using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF2Mesh
{
    class bf2Mesh
    {
        // header
        bf2head head;

        // unknown
        byte u1; // always 0?

        // geoms
        int geomnum;                      // numer of geoms
        bf2geom[] geom;                    // geom array

        // vertex attribute table
        int vertattribnum;                // number of vertex attribute table entries
        bf2attrib[] vertattrib;            // array of vertex attribute table entries

        // vertices
        int vertformat;                   // always 4?  (e.g. GL_FLOAT)
        int vertstride;                   // vertex stride
        int vertnum;                      // number of vertices in buffer
        float[] vert;                      // vertex array

        // indices
        int indexnum;                     // number of indices

        UInt16[] index;            // index array

        // unknown
        int u2;                           // always 8?


        // internal/hacks
        bool isSkinnedMesh;
        bool isBundledMesh;
        bool isBFP4F;


        public bf2Mesh Load(string filename)
        {
            Debug.Assert(filename != null);

            if (!File.Exists(filename))
            {
                Console.WriteLine("File " + filename + " was not found.");
                return null;
            }


            // open file
            using (BinaryReader file = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                if (file == null)
                    return null;

                // header
                head.u1 = file.ReadUInt32();
                head.version = file.ReadUInt32();
                head.u3 = file.ReadUInt32();
                head.u4 = file.ReadUInt32();
                head.u5 = file.ReadUInt32();
                Console.WriteLine("head start at " + file.BaseStream.Position);



                Console.WriteLine(" u1: " + head.u1);
                Console.WriteLine(" version: " + head.version);
                Console.WriteLine(" u3: " + head.u3);
                Console.WriteLine(" u4: " + head.u4);
                Console.WriteLine(" u5: " + head.u5);
                Console.WriteLine("head end at " + file.BaseStream.Position);




                // unknown (1 byte)
                // stupid little byte that misaligns the entire file!
                head.u1 = file.ReadByte();
                Console.WriteLine(" u1: " + head.u1);


                // for BFP4F, the value is "1", so perhaps this is a version number as well
                if (head.u1 == 1) isBFP4F = true;

                // --- geom table ---------------------------------------------------------------------------

                Console.WriteLine("geom table start at " + file.BaseStream.Position);

                // geomnum (4 bytes)
                geomnum = file.ReadInt32();
                Console.WriteLine("geomnum: " + geomnum);

                // geom table (4 bytes * groupnum)
                geom = new bf2geom[geomnum];
                for (int i = 0; i < geomnum; i++)
                {
                    geom[i].read(file, head.version);
                }

                Console.WriteLine("geom table end at " + file.BaseStream.Position);



                // --- vertex attribute table -------------------------------------------------------------------------------

                Console.WriteLine("attrib block at  " + file.BaseStream.Position);

                // vertattribnum (4 bytes)
                vertattribnum = file.ReadInt32();
                Console.WriteLine(" vertattribnum: " + vertattribnum);

                // vertex attributes
                vertattrib = new bf2attrib[vertattribnum];


                for (int i = 0; i < vertattribnum; i++)
                {
                    vertattrib[i].flag = file.ReadUInt16();
                    vertattrib[i].offset = file.ReadUInt16();
                    vertattrib[i].vartype = file.ReadUInt16();
                    vertattrib[i].usage = file.ReadUInt16();

                    Console.WriteLine(" attrib[" + i + "]: " + vertattrib[i].flag + " " + vertattrib[i].offset + " " + vertattrib[i].vartype + " " + vertattrib[i].usage);
                }

                Console.WriteLine("attrib block end at " + file.BaseStream.Position);


                // --- vertices -----------------------------------------------------------------------------

                Console.WriteLine("vertex block start at " + file.BaseStream.Position);


                vertformat = file.ReadInt32();
                vertstride = file.ReadInt32();
                vertnum = file.ReadInt32();


                Console.WriteLine(" vertformat: " + vertformat);
                Console.WriteLine(" vertstride: " + vertstride);
                Console.WriteLine(" vertnum: " + vertnum);

                vert = new float[vertnum * vertstride];

                for (int i = 0; i < vertnum * (vertstride / vertformat); i++)
                {
                    vert[i] = file.ReadSingle();

                    //Console.WriteLine(i + " :" + vert[i]);
                }


                Console.WriteLine("vertex block end at " + file.BaseStream.Position);


                // --- indices ------------------------------------------------------------------------------

                Console.WriteLine("index block start at " + file.BaseStream.Position);


                indexnum = file.ReadInt32();
                Console.WriteLine(" indexnum: " + indexnum);

                index = new ushort[indexnum];

                for (int i = 0; i < indexnum; i++)
                {
                    index[i] = file.ReadUInt16();
                }

                Console.WriteLine("index block end at " + file.BaseStream.Position);

                // --- rigs -------------------------------------------------------------------------------

                // unknown (4 bytes)
                if (!isSkinnedMesh)
                {
                    u2 = file.ReadInt32();
                    Console.WriteLine("u2: " + u2);

                }

                // rigs/nodes
                Console.WriteLine("nodes chunk start at " + file.BaseStream.Position);
                for (int i = 0; i < geomnum; i++)
                {
                    if (i > 0) Console.WriteLine("");
                    Console.WriteLine(" geom " + i + " start");
                    for (int j = 0; j < geom[i].lodnum; j++)
                    {
                        Console.WriteLine("  lod " + j + " start");
                        geom[i].lod[j].readNodeData(file, head.version, isSkinnedMesh, isBundledMesh);
                        Console.WriteLine("  lod " + j + " end");
                    }
                    Console.WriteLine(" geom " + i + " end");
                }
                Console.WriteLine("nodes chunk end at " + file.BaseStream.Position);



                // --- geoms ------------------------------------------------------------------------------

                for (int i = 0; i < geomnum; i++)
                {
                    Console.WriteLine("geom " + i + " start at " + file.BaseStream.Position);
                    //geom[i].ReadMatData( fp, head.version );

                    for (int j = 0; j < geom[i].lodnum; j++)
                    {
                        Console.WriteLine(" lod " + j + " start");
                        geom[i].lod[j].readMatData(file, head.version, isSkinnedMesh);
                        Console.WriteLine(" lod " + j + " end");
                    }

                    Console.WriteLine("geom " + i + " block end at " + file.BaseStream.Position);
                    Console.WriteLine("");
                }

                // --- end of file -------------------------------------------------------------------------

                Console.WriteLine("done reading " + file.BaseStream.Position);

                file.BaseStream.Seek(0, SeekOrigin.End);
                Console.WriteLine("file size is " + file.BaseStream.Position);
                Console.WriteLine("");

            }
            return null;

        }

    }

}