using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RewFile
{
    //  DWORD           4 bytes uint
    //  LONG            4 bytes int
    //  WORD            2 bytes short
    //  CIEXYZTRIPLE    ? bytes object
    using System.Runtime.InteropServices;
    using System.Security.Policy;
    using System.Windows.Markup;

    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapInfo
    {
        public BitmapInfoHeader Header;
        public RGBQuad[] Colors;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BitmapInfoHeader
    {
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public uint SizeImage;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        public uint ClrUsed;
        public uint ClrImportant;
        //V3
        public uint RedMask;
        public uint GreenMask;
        public uint BlueMask;
        public uint AlphaMask;
        public uint CSType;
        public CIEXYZTRIPLE ciexyzTriple;
        public uint GammaRed;
        public uint GammaGreen;
        public uint GammaBlue;
        public uint Intent;
        public uint ProfileData;
        public uint ProfileSize;
        public uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RGBQuad
    {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CIEXYZTRIPLE
    {
        public CIEXYZ ciexyzRed;
        public CIEXYZ ciexyzGreen;
        public CIEXYZ ciexyzBlue;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CIEXYZ
    {
        public uint ciexyzX;
        public uint ciexyzY;
        public uint ciexyzZ;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapV2InfoHeader
    {
        //Should equal 124
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public uint SizeImage;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        //Guess work v
        public uint ClrUsed;
        public uint ClrImportant;
        //Typical end of DIB header at index 40 (54 including BMP header)
        public uint BlueMask;
        public uint GreenMask;
        public uint RedMask;
        public uint AlphaMask;
        //Enum? object? or big uint value
        public uint CSType;
        public uint ciexyzX;
        public uint ciexyzY;
        public uint ciexyzZ;
        public uint ciexyzX2;
        public uint ciexyzY2;
        public uint ciexyzZ2;
        public uint ciexyzX3;
        public uint ciexyzY3;
        public uint ciexyzZ3;
        public uint GammaRed;
        public uint GammaGreen;
        public uint GammaBlue;
        //  There was a bit of data at the end
        /*  Index 60: 0       // Intent?
            Index 61: 0
            Index 62: 0
            Index 63: 0
            Index 0: 0        // ProfileData?
            Index 1: 0
            Index 2: 0
            Index 3: 0
            Index 4: 0        // ProfileSize?
            Index 5: 0         
            Index 6: 0        
            Index 7: 0        
            Index 8: 0        // Reserved ushort?
            Index 9: 0        
            Index 10: 255     // red
            Index 11: 0
            Index 12: 0
            Index 13: 0
            Index 14: 0
            Index 15: 255     // green
            Index 16: 0
            Index 17: 0
            Index 18: 0
            Index 19: 0
            Index 20: 255     // blue
            Index 21: 0       // End of data in V2? maybe with pixel data being appended after this
            Index 22: 0       // Maybe alpha is appended here in V3
            Index 23: 0
            Index 24: 0
            Index 25: 255
         */
        public static BitmapV2InfoHeader getHeader(byte[] buffer)
        {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            BitmapV2InfoHeader data = (BitmapV2InfoHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(BitmapV2InfoHeader));
            handle.Free();
            return data;
        }
        public static byte[] CreateDIBHeader(REW rew, out BitmapV2InfoHeader header)
        {
            header = new BitmapV2InfoHeader();
            uint size = 124;
            byte[] array = new byte[size];
            Array.Copy(BitConverter.GetBytes(size), 0, array, 0, 4);
            Array.Copy(BitConverter.GetBytes(header.Width = (int)rew.Width), 0, array, 4, 4);
            Array.Copy(BitConverter.GetBytes(header.Height = (int)rew.Height), 0, array, 8, 4);
            Array.Copy(BitConverter.GetBytes(header.Planes = 1), 0, array, 10, 2);
            Array.Copy(BitConverter.GetBytes(header.BitCount = (ushort)rew.BitsPerPixel), 0, array, 12, 2);
            Array.Copy(BitConverter.GetBytes(header.Compression = (uint)BitmapCompressionMode.BI_BITFIELDS), 0, array, 16, 4);
            Array.Copy(BitConverter.GetBytes(header.SizeImage = (uint)rew.RealLength), 0, array, 20, 4);
            Array.Copy(BitConverter.GetBytes(header.XPelsPerMeter = 96), 0, array, 24, 4);
            Array.Copy(BitConverter.GetBytes(header.YPelsPerMeter = 96), 0, array, 28, 4);
            Array.Copy(BitConverter.GetBytes(header.ClrUsed = 0), 0, array, 32, 4);
            Array.Copy(BitConverter.GetBytes(header.ClrImportant = 0), 0, array, 36, 4);
            Array.Copy(BitConverter.GetBytes(header.BlueMask = 0x000000FF), 0, array, 40, 4);
            Array.Copy(BitConverter.GetBytes(header.GreenMask = 0x0000FF00), 0, array, 44, 4);
            Array.Copy(BitConverter.GetBytes(header.RedMask = 0x00FF0000), 0, array, 48, 4);
            Array.Copy(BitConverter.GetBytes(header.AlphaMask = 0xFF000000), 0, array, 52, 4);
            header.CSType = BitConverter.ToUInt32(new byte[] { 32, 110, 106, 87 }, 0);
            Array.Copy(new byte[] { 32, 110, 106, 87 }, 0, array, 56, 4);
            //  At this point, the rest of the header is basically all 0's
            Array.Copy(new byte[64], 0, array, 60, 64);
            return array;
            //Array.Copy(BitConverter.GetBytes(header.ciexyzX), 0, array, 60, 4);
            //Array.Copy(BitConverter.GetBytes(header.ciexyzY), 0, array, 64, 4);
            //Array.Copy(BitConverter.GetBytes(header.ciexyzX2), 0, array, 68, 4);
            //Array.Copy(BitConverter.GetBytes(header.ciexyzY2), 0, array, 72, 4);
            //Array.Copy(BitConverter.GetBytes(header.ciexyzX3), 0, array, 76, 4);
            //Array.Copy(BitConverter.GetBytes(header.ciexyzY3), 0, array, 80, 4);
            //Array.Copy(BitConverter.GetBytes(header.GammaRed = 0), 0, array, 84, 4);
            //Array.Copy(BitConverter.GetBytes(header.GammaGreen = 0), 0, array, 88, 4);
            //Array.Copy(BitConverter.GetBytes(header.GammaBlue = 0), 0, array, 92, 4);
            //return array;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;

        public void Init()
        {
            biSize = (uint)Marshal.SizeOf(this);
        }

        public static readonly uint HeaderOffset = 40;
        public static byte[] CreateDIBHeader(BITMAPINFOHEADER header)
        {
            byte[] array =
                //Size of DIB header
                BitConverter.GetBytes(HeaderOffset)
                //Image width
                .Concat(BitConverter.GetBytes(header.biWidth))
                //Image height
                .Concat(BitConverter.GetBytes(header.biHeight))
                //# of color planes being used
                .Concat(BitConverter.GetBytes(header.biPlanes))
                //Pixel format
                .Concat(BitConverter.GetBytes(header.biBitCount))
                //Compression, if raw, normally 0
                .Concat(BitConverter.GetBytes((uint)header.biCompression))
                //Size of the pixel array (including padding)
                .Concat(BitConverter.GetBytes(header.biSizeImage))
                //Horizontal resolution of the image (96)
                .Concat(BitConverter.GetBytes(header.biXPelsPerMeter))
                //Vertical resolution of the image (96)
                .Concat(BitConverter.GetBytes(header.biYPelsPerMeter))
                //# of colors in the color palette
                .Concat(BitConverter.GetBytes(header.biClrUsed))
                //# of important colors used (0 means all)
                .Concat(BitConverter.GetBytes(header.biClrImportant))
                .ToArray();
            return array;
        }
        public static byte[] CreateDIBHeader(REW image, out BITMAPINFOHEADER header)
        {
            header = new BITMAPINFOHEADER();
            byte[] array =
                //Size of DIB header
                BitConverter.GetBytes(HeaderOffset)
                //Image width
                .Concat(BitConverter.GetBytes(header.biWidth = (int)image.Width))
                //Image height
                .Concat(BitConverter.GetBytes(header.biHeight = (int)image.Height))
                //# of color planes being used
                .Concat(BitConverter.GetBytes(header.biPlanes = (ushort)0))
                //Pixel format
                .Concat(BitConverter.GetBytes(header.biBitCount = (ushort)image.BitsPerPixel))
                //Compression, if raw, normally 0
                .Concat(BitConverter.GetBytes(header.biCompression = (uint)BitmapCompressionMode.BI_RGB))
                //Size of the pixel array (including padding)
                .Concat(BitConverter.GetBytes(header.biSizeImage = (uint)image.RealLength))
                //Horizontal resolution of the image (96)
                .Concat(BitConverter.GetBytes(header.biXPelsPerMeter = 0))
                //Vertical resolution of the image (96)
                .Concat(BitConverter.GetBytes(header.biYPelsPerMeter = 0))
                //# of colors in the color palette
                .Concat(BitConverter.GetBytes(header.biClrUsed = 0))
                //# of important colors used (0 means all)
                .Concat(BitConverter.GetBytes(header.biClrImportant = 0))
                .ToArray();
            return array;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPV3INFOHEADER    //BITMAPV3INFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
        public uint biRedMask;
        public uint biGreenMask;
        public uint biBlueMask;
        public uint biAlphaMask;
        public uint biCSType;
        public uint ciexyzX;
        public uint ciexyzY;
        public uint ciexyzZ;
        public uint ciexyzX2;
        public uint ciexyzY2;
        public uint ciexyzZ2;
        public uint ciexyzX3;
        public uint ciexyzY3;
        public uint ciexyzZ3;
        public uint biGammaRed;
        public uint biGammaGreen;
        public uint biGammaBlue;
        public uint biIntent;
        public uint biProfileData;
        public uint biProfileSize;
        public uint biReserved;

        public void Init()
        {
            biSize = (uint)Marshal.SizeOf(this);
        }
        public static byte[] CreateDIBHeader(BITMAPV3INFOHEADER header)
        {
            int num = 0;
            uint size = (uint)Marshal.SizeOf(header);
            byte[] array = new byte[size];
            Array.Copy(BitConverter.GetBytes(size), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biWidth), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biHeight), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biPlanes), 0, array, num, num += 2);
            Array.Copy(BitConverter.GetBytes(header.biBitCount), 0, array, num, num += 2);
            Array.Copy(BitConverter.GetBytes(header.biCompression), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biSizeImage), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biXPelsPerMeter), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biYPelsPerMeter), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biClrUsed), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biClrImportant), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biRedMask), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGreenMask), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biBlueMask), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biCSType), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biCSType), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzX), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzY), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzX2), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzY2), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzX3), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzY3), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaRed), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaGreen), 0, array, num, num += 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaBlue), 0, array, num, num += 4);
            return array;
        }
        public static byte[] CreateDIBHeader(REW rew, out BITMAPV3INFOHEADER header)
        {
            int num = -4;
            header = new BITMAPV3INFOHEADER();
            uint size = (uint)Marshal.SizeOf(header);
            byte[] array = new byte[56];
            Array.Copy(BitConverter.GetBytes(56), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biWidth = (int)rew.Width), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biHeight = (int)rew.Height), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biPlanes = 1), 0, array, num += 2, 2);
            Array.Copy(BitConverter.GetBytes(header.biBitCount = 32), 0, array, num += 2, 2);
            Array.Copy(BitConverter.GetBytes(header.biCompression = (uint)BitmapCompressionMode.BI_BITFIELDS), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biSizeImage = (uint)rew.RealLength), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biXPelsPerMeter = 96), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biYPelsPerMeter = 96), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biClrUsed = 0), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biClrImportant = 0), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biRedMask = 0x00FF0000), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biGreenMask = 0x0000FF00), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biBlueMask = 0x000000FF), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biAlphaMask = 0xFF000000), 0, array, num += 4, 4);
            header.biCSType = BitConverter.ToUInt32(new byte[] { 32, 110, 106, 87 }, 0);
            Array.Copy(new byte[] { 32, 110, 106, 87 }, 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzX), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzY), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzX2), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzY2), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzX3), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.ciexyzY3), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaRed = 0), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaGreen = 0), 0, array, num += 4, 4);
            Array.Copy(BitConverter.GetBytes(header.biGammaBlue = 0), 0, array, num += 4, 4);
            //Array.Copy(BitConverter.GetBytes(header.biIntent = 0), 0, array, num += 4, 4);
            //Array.Copy(BitConverter.GetBytes(header.biProfileData = 0), 0, array, num += 4, 4);
            //Array.Copy(BitConverter.GetBytes(header.biProfileSize = 0), 0, array, num += 4, 4);
            //Array.Copy(BitConverter.GetBytes(header.biReserved = 0), 0, array, num, 4);
            return array;
        }
    }
    //BITMAPV3INFOHEADER, * PBITMAPV3INFOHEADER;
}
