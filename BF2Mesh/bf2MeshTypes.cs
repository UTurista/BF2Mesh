using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF2Mesh
{
    // three dimensional floating point vector
    struct float3 // 12 bytes
    {
        public float x;
        public float y;
        public float z;
    }

    // bounding box
    struct aabb // 24 bytes
    {
        public float3 min;
        public float3 max;
    }

    // 4x4 transformation matrix
    class matrix4 // 64 bytes
    {
        public matrix4() { this.m = new float[4 * 4]; }
        public float[] m;
    }

    // bf2 mesh file header
    struct BF2Head              // 20 bytes
    {
        public uint u1;         // 0
        public uint version;    // 10 for most bundledmesh, 6 for some bundledmesh, 11 for staticmesh
        public uint u3;         // 0
        public uint u4;         // 0
        public uint u5;         // 0
    }


    // vertex attribute table entry
    struct bf2attrib            // 8 bytes
    {
        public UInt16 flag;     // some sort of boolean flag (if true the below field are to be ignored?)
        public UInt16 offset;   // offset from vertex data start
        public UInt16 vartype;  // attribute type (vec2, vec3 etc)
        public UInt16 usage;    // usage ID (vertex, texcoord etc)

        // Note: "usage" field correspond to the definition in DX SDK "Include\d3d9types.h"
        //       Looks like DICE extended these for additional UV channels, these constants
        //       are much larger to avoid conflict with core DX enums.
    }

    // bone structure
    struct bf2bone                  // 68 bytes
    {
        public uint id;             //  4 bytes
        public matrix4 transform;   // 64 bytes
    }

    // rig structure
    struct bf2rig
    {
        public int bonenum;
        public bf2bone[] bone;

        // functions
        public bool read(BinaryReader file, uint version)
        {
            // bonenum (4 bytes)
            bonenum = file.ReadInt32();
            Console.WriteLine("   bonenum: " + bonenum);

            Debug.Assert(bonenum >= 0);
            Debug.Assert(bonenum < 99);

            // bones (68 bytes * bonenum)
            if (bonenum > 0)
            {
                bone = new bf2bone[bonenum];

                for (int i = 0; i < bonenum; i++)
                {
                    bone[i].id = file.ReadUInt32();    //  4 bytes

                    // 64 bytes
                    for (int mi = 0; mi < 4; mi++)
                    {
                        for (int mj = 0; mj < 4; mj++)
                        {
                            if (bone[i].transform == null)
                            {
                                bone[i].transform = new matrix4();
                            }
                            bone[i].transform.m[mi * mj] = file.ReadSingle();
                        }
                    }
                    Console.WriteLine("   boneid[" + i + "]: " + bone[i].id);
                }
            }

            return true; // success
        }
    }

    // material (aka drawcall)
    struct bf2mat
    {
        public uint alphamode;         // 0=opaque, 1=blend, 2=alphatest
        public string fxfile;          // shader filename string
        public string technique;       // technique name

        // texture map filenames
        public int mapnum;             // number of texture map filenames
        public string[] map;           // map filename array

        // geometry info
        public uint vstart;            // vertex start offset
        public uint istart;            // index start offset
        public uint inum;              // number of indices
        public uint vnum;              // number of vertices

        // misc
        uint u4;                // always 1?
        uint u5;                // always 0x34E9?
        uint u6;                // most often 18/19
        public aabb bounds;            // per-material bounding box (StaticMesh only)

        // functions
        public bool read(BinaryReader file, uint version, bool isSkinnedMesh)
        {
            // alpha flag (4 bytes)
            if (!isSkinnedMesh)
            {
                alphamode = file.ReadUInt32();
                Console.WriteLine("   alphamode: " + alphamode);
            }

            // fx filename
            int strSize = file.ReadInt32();
            char[] characters = file.ReadChars(strSize);
            fxfile = new string(characters);
            Console.WriteLine("   fxfile: " + fxfile);

            // material name
            strSize = file.ReadInt32();
            characters = file.ReadChars(strSize);
            technique = new string(characters);
            Console.WriteLine("   matname: " + technique);

            // mapnum (4 bytes)
            mapnum = file.ReadInt32();
            Console.WriteLine("   mapnum: " + mapnum);
            Debug.Assert(mapnum >= 0);
            Debug.Assert(mapnum < 99);

            // mapnames
            if (mapnum > 0)
            {
                map = new string[mapnum];
                for (int i = 0; i < mapnum; i++)
                {
                    strSize = file.ReadInt32();
                    characters = file.ReadChars(strSize);
                    map[i] = new string(characters);
                    Console.WriteLine("    map " + i + ": " + map[i]);
                }
            }

            // geometry info
            vstart = file.ReadUInt32();
            istart = file.ReadUInt32();
            inum = file.ReadUInt32();
            vnum = file.ReadUInt32();

            Console.WriteLine("   vstart: " + vstart);
            Console.WriteLine("   istart: " + istart);
            Console.WriteLine("   inum: " + inum);
            Console.WriteLine("   vnum: " + vnum);

            // unknown
            // TODO: actually its 2x uint32
            u4 = file.ReadUInt32();
            u5 = file.ReadUInt16();
            u6 = file.ReadUInt16();

            // bounds
            if (!isSkinnedMesh)
            {
                if (version == 11)
                {
                    bounds.min.x = file.ReadSingle();
                    bounds.min.y = file.ReadSingle();
                    bounds.min.z = file.ReadSingle();

                    bounds.max.x = file.ReadSingle();
                    bounds.max.y = file.ReadSingle();
                    bounds.max.z = file.ReadSingle();
                }
            }

            // success
            return true;
        }
    }


    // lod, holds mainly a collection of materials
    struct bf2lod
    {
        // bounding box
        public float3 min;
        public float3 max;
        public float3 pivot;       // not sure this is really a pivot (only on version<=6)

        // skinning matrices (SkinnedMesh only)
        public int rignum;         // this usually corresponds to meshnum (but what was meshnum again??)
        public bf2rig[] rig;       // array of rigs

        // nodes (staticmesh and bundledmesh only)
        public int nodenum;
        public matrix4[] node;

        // material/drawcalls
        public int matnum;         // number of materials
        public bf2mat[] mat;       // material array

        // functions
        public bool readNodeData(BinaryReader file, uint version, bool isSkinnedMesh, bool isBundledMesh)
        {
            // bounds (24 bytes)   
            min.x = file.ReadSingle();
            min.y = file.ReadSingle();
            min.z = file.ReadSingle();

            max.x = file.ReadSingle();
            max.y = file.ReadSingle();
            max.z = file.ReadSingle();

            // unknown (12 bytes)
            if (version <= 6)
            { // version 4 and 6
                pivot.x = file.ReadSingle();
                pivot.y = file.ReadSingle();
                pivot.z = file.ReadSingle();
            }

            // skinnedmesh has different rigs
            if (isSkinnedMesh)
            {
                // rignum (4 bytes)
                rignum = file.ReadInt32();
                Console.WriteLine("  rignum: " + rignum);

                // read rigs
                if (rignum > 0)
                {
                    rig = new bf2rig[rignum];
                    for (int i = 0; i < rignum; i++)
                    {
                        Console.WriteLine("  rig block " + i + " start at " + file.BaseStream.Position);
                        rig[i].read(file, version);
                        Console.WriteLine("  rig block " + i + " end at " + file.BaseStream.Position);
                    }
                }
            }
            else
            {

                // nodenum (4 bytes)
                nodenum = file.ReadInt32();
                Console.WriteLine("   nodenum: " + nodenum);

                // node matrices (64 bytes * nodenum)
                if (!isBundledMesh)
                {
                    Console.WriteLine("   node data");
                    if (nodenum > 0)
                    {
                        node = new matrix4[nodenum];

                        for (int i = 0; i < nodenum; i++)
                        {
                            node[i] = new matrix4();
                            for (int mi = 0; mi < 4; mi++)
                            {
                                for (int mj = 0; mj < 4; mj++)
                                {
                                    node[i].m[mi * mj] = file.ReadSingle();
                                }
                            }
                        }
                    }
                }
            }

            // success
            return true;
        }
        public bool readMatData(BinaryReader file, uint version, bool isSkinnedMesh)
        {
            // matnum (4 bytes)
            matnum = file.ReadInt32();
            Console.WriteLine("  matnum: " + matnum);

            Debug.Assert(matnum >= 0);
            Debug.Assert(matnum < 99);

            // materials (? bytes)
            if (matnum > 0)
            {
                mat = new bf2mat[matnum];
                for (int i = 0; i < matnum; i++)
                {
                    Console.WriteLine("  mat " + i + " start at " + file.BaseStream.Position);
                    if (!mat[i].read(file, version, isSkinnedMesh)) return false;
                    Console.WriteLine("  mat "+i+" end at " + file.BaseStream.Position);
                }
            }

            return true; // success
        }
    }

    // geom, holds a collection of LODs
    struct bf2geom
    {
        public int lodnum;      // number of LODs
        public bf2lod[] lod;    // array of LODs

        // functions
        public bool read(BinaryReader file, uint version)
        {
            // lodnum (4 bytes)
            lodnum = file.ReadInt32();
            Console.WriteLine("lodnum: " + lodnum);

            Debug.Assert(lodnum >= 0);
            Debug.Assert(lodnum < 99);

            // allocate lods
            if (lodnum > 0)
            {
                lod = new bf2lod[lodnum];
            }

            return true;  // success         
        }
    }
}