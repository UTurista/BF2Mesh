using System;
using System.Globalization;
using System.IO;
using System.Text;

[Flags]
public enum DdsHeaderFlags
{
  Caps = 0x1, // DDSD_CAPS
  Height = 0x2, // DDSD_HEIGHT
  Width = 0x4, // DDSD_WIDTH
  Pitch = 0x8, // DDSD_PITCH
  PixelFormat = 0x1000, // DDSD_PIXELFORMAT
  Mipmap = 0x20000, // DDSD_MIPMAPCOUNT
  LinearSize = 0x80000, // DDSD_LINEARSIZE
  Volume = 0x800000, // DDSD_DEPTH
  Texture = Caps | Height | Width | PixelFormat
}

[Flags]
public enum DdsPixelFormatFlags
{
  AlphaPixels = 0x00000001, // DDPF_ALPHAPIXELS
  Alpha = 0x00000002, // DDPF_ALPHA
  FourCC = 0x00000004, // DDPF_FOURCC
  Rgb = 0x00000040, // DDPF_RGB
  Pal8 = 0x00000020, // DDPF_PALETTEINDEXED8
  Yuv = 0x00000200, // DDPF_YUV
  Luminance = 0x00020000, // DDPF_LUMINANCE
  Rgba = Rgb | AlphaPixels,
  LuminanceAlpha = Luminance | AlphaPixels
}

[Flags]
public enum DdsSurfaceFlags
{
  Cubemap = 0x8, // DDSCAPS_COMPLEX
  Texture = 0x1000, // DDSCAPS_TEXTURE
  Mipmap = Cubemap | 0x400000
}

[Flags]
public enum DdsCubemapFlags
{
  CubeMap = 0x00000200, // DDSCAPS2_CUBEMAP
  PositiveX = 0x00000600, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX
  NegativeX = 0x00000a00, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX
  PositiveY = 0x00001200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY
  NegativeY = 0x00002200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY
  PositiveZ = 0x00004200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ
  NegativeZ = 0x00008200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ
  AllFaces = PositiveX | NegativeX | PositiveY | NegativeY | PositiveZ | NegativeZ,
  Volume = 0x00200000 // DDSCAPS2_VOLUME
}

public class DdsHeader
{
  public DdsHeaderFlags Flags { get; set; }
  public uint Height { get; set; }
  public uint Width { get; set; }
  public uint PitchOrLinearSize { get; set; }
  public uint Depth { get; set; }
  public uint MipMapCount { get; set; }
  public DdsPixelFormat PixelFormat { get; set; }
  public DdsSurfaceFlags SurfaceFlags;
  public DdsCubemapFlags CubemapFlags;

  public override string ToString()
  {
    using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture)) {
      sw.WriteLine("Flags: " + Flags);
      sw.WriteLine("Height: " + Height);
      sw.WriteLine("Width: " + Width);
      sw.WriteLine("PitchOrLinearSize: " + PitchOrLinearSize);
      sw.WriteLine("Depth: " + Depth);
      sw.WriteLine("MipMapCount: " + MipMapCount);
      sw.WriteLine("PixelFormat:\n" + PixelFormat.ToString(true));
      sw.WriteLine("SurfaceFlags: " + SurfaceFlags);
      sw.WriteLine("CubemapFlags: " + CubemapFlags);

      return sw.ToString();
    }
  }
}

public class DdsPixelFormat
{
  public DdsPixelFormatFlags Flags { get; set; }
  public string FourCC { get; set; }
  public uint RGBBitCount { get; set; }
  public uint RBitMask { get; set; }
  public uint GBitMask { get; set; }
  public uint BBitMask { get; set; }
  public uint ABitMask { get; set; }

  public string ToString(bool indented)
  {
    using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture)) {
      sw.WriteLine(String.Format("{0}Flags: {1}", indented ? "\t" : String.Empty, Flags));
      sw.WriteLine(String.Format("{0}FourCC: {1}", indented ? "\t" : String.Empty, FourCC));
      sw.WriteLine(String.Format("{0}RGBBitCount: {1}", indented ? "\t" : String.Empty, RGBBitCount));
      sw.WriteLine(String.Format("{0}RBitMask: {1}", indented ? "\t" : String.Empty, RBitMask));
      sw.WriteLine(String.Format("{0}GBitMask: {1}", indented ? "\t" : String.Empty, GBitMask));
      sw.WriteLine(String.Format("{0}BBitMask: {1}", indented ? "\t" : String.Empty, BBitMask));
      sw.Write(String.Format("{0}ABitMask: {1}", indented ? "\t" : String.Empty, ABitMask));

      return sw.ToString();
    }
  }

  public override string ToString()
  {
    return ToString(false);
  }
}

public class Dds
{
  public static DdsHeader GetHeader(FileStream fs)
  {
    if (!fs.CanSeek || !fs.CanRead)
      throw new NotSupportedException("FileStream must be seekable and readable");

    fs.Seek(0, SeekOrigin.Begin);

    uint magicNumber = ReadDword(fs);
    if (magicNumber != 0x20534444)
      return null;

    uint size = ReadDword(fs);
    if (size != 124)
      return null;

    uint flags = ReadDword(fs);
    uint height = ReadDword(fs);
    uint width = ReadDword(fs);
    uint pitchOrLinearSize = ReadDword(fs);
    uint depth = ReadDword(fs);
    uint mipMapCount = ReadDword(fs);
    /*uint[] reserved1 = */ReadDwords(fs, 11);
    uint[] pixelFormat = ReadDwords(fs, 8);
    uint caps1 = ReadDword(fs);
    uint caps2 = ReadDword(fs);
    /*uint caps3 = */ReadDword(fs);
    /*uint caps4 = */ReadDword(fs);
    /*uint reserved2 = */ReadDword(fs);

    return new DdsHeader() {
      Flags = (DdsHeaderFlags)flags,
      Height = height,
      Width = width,
      PitchOrLinearSize = pitchOrLinearSize,
      Depth = depth,
      MipMapCount = mipMapCount,
      PixelFormat = new DdsPixelFormat() {
        Flags = (DdsPixelFormatFlags)pixelFormat[1],
        FourCC = Encoding.ASCII.GetString(BitConverter.GetBytes(pixelFormat[2])),
        RGBBitCount = pixelFormat[3],
        RBitMask = pixelFormat[4],
        GBitMask = pixelFormat[5],
        BBitMask = pixelFormat[6],
        ABitMask = pixelFormat[7]
      },
      SurfaceFlags = (DdsSurfaceFlags)caps1,
      CubemapFlags = (DdsCubemapFlags)caps2,
    };
  }

  public static bool IsBadSize(DdsHeader header)
  {
    if (header.MipMapCount > 0)
      if (!IsPowerOfTwo(header.Width) || !IsPowerOfTwo(header.Height))
        return true;

    return false;
  }

  private static bool IsPowerOfTwo(ulong dimension)
  {
    return (dimension != 0) && ((dimension & (dimension - 1)) == 0);
  }

  private static uint ReadDword(FileStream fs)
  {
    byte[] buffer = new byte[4];
    fs.Read(buffer, 0, 4);
    return BitConverter.ToUInt32(buffer, 0);
  }

  private static uint[] ReadDwords(FileStream fs, int length)
  {
    byte[] buffer = new byte[length * 4];
    fs.Read(buffer, 0, length * 4);

    uint[] dwords = new uint[length];
    for (int i = 0; i < dwords.Length; i++)
      dwords[i] = BitConverter.ToUInt32(new byte[] { buffer[i * 4], buffer[i * 4 + 1], buffer[i * 4 + 2], buffer[i * 4 + 3] }, 0);

    return dwords;
  }
}