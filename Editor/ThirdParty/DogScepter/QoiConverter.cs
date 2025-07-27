// Adapted from: https://github.com/colinator27/DogScepter/blob/master/DogScepterLib/Project/Util/QoiConverter.cs

namespace DogScepterLib.Project.Util;

// Responsible for QOI image format conversions to Bitmaps
// Note that the QOI format used in GameMaker is modified significantly:
//  - It's little-endian and has a different header, with missing/added fields.
//  - The DIFF encoding works differently.
// Derived from official QOI source at https://github.com/phoboslab/qoi and QoiSharp at https://github.com/NUlliiON/QoiSharp.
public static class QoiConverter
{
    private const byte QOI_INDEX = 0x00;
    private const byte QOI_RUN_8 = 0x40;
    private const byte QOI_RUN_16 = 0x60;
    private const byte QOI_DIFF_8 = 0x80;
    private const byte QOI_DIFF_16 = 0xc0;
    private const byte QOI_DIFF_24 = 0xe0;

    private const byte QOI_COLOR = 0xf0;
    private const byte QOI_MASK_2 = 0xc0;
    private const byte QOI_MASK_3 = 0xe0;
    private const byte QOI_MASK_4 = 0xf0;

    public static byte[] GetImageFromSpan(Span<byte> data, out int width, out int height )
    {
        if (data[0] != (byte)'f' || data[1] != (byte)'i' || data[2] != (byte)'o' || data[3] != (byte)'q')
            throw new Exception("Invalid little-endian QOIF image magic");

        width = data[4] + (data[5] << 8);
        height = data[6] + (data[7] << 8);
        int endPos = 12 + data[8] + (data[9] << 8) + (data[10] << 16) + (data[11] << 24);

        var img = new byte[width * height * 4];
        int imgPos = 0;

        int pos = 12;
        int run = 0;
        byte r = 0, g = 0, b = 0, a = 255;
        Span<byte> index = stackalloc byte[64 * 4];
        while (imgPos < img.Length)
        {
            if (run > 0)
            {
                run--;
            }
            else if (pos < endPos)
            {
                int b1 = data[pos++];

                if ((b1 & QOI_MASK_2) == QOI_INDEX)
                {
                    int indexPos = (b1 ^ QOI_INDEX) << 2;
                    r = index[indexPos];
                    g = index[indexPos + 1];
                    b = index[indexPos + 2];
                    a = index[indexPos + 3];
                }
                else if ((b1 & QOI_MASK_3) == QOI_RUN_8)
                {
                    run = b1 & 0x1f;
                }
                else if ((b1 & QOI_MASK_3) == QOI_RUN_16)
                {
                    int b2 = data[pos++];
                    run = (((b1 & 0x1f) << 8) | b2) + 32;
                }
                else if ((b1 & QOI_MASK_2) == QOI_DIFF_8)
                {
                    r += (byte)(((b1 & 48) << 26 >> 30) & 0xff);
                    g += (byte)(((b1 & 12) << 28 >> 22 >> 8) & 0xff);
                    b += (byte)(((b1 & 3) << 30 >> 14 >> 16) & 0xff);
                }
                else if ((b1 & QOI_MASK_3) == QOI_DIFF_16)
                {
                    int b2 = data[pos++];
                    int merged = b1 << 8 | b2;
                    r += (byte)(((merged & 7936) << 19 >> 27) & 0xff);
                    g += (byte)(((merged & 240) << 24 >> 20 >> 8) & 0xff);
                    b += (byte)(((merged & 15) << 28 >> 12 >> 16) & 0xff);
                }
                else if ((b1 & QOI_MASK_4) == QOI_DIFF_24)
                {
                    int b2 = data[pos++];
                    int b3 = data[pos++];
                    int merged = b1 << 16 | b2 << 8 | b3;
                    r += (byte)(((merged & 1015808) << 12 >> 27) & 0xff);
                    g += (byte)(((merged & 31744) << 17 >> 19 >> 8) & 0xff);
                    b += (byte)(((merged & 992) << 22 >> 11 >> 16) & 0xff);
                    a += (byte)(((merged & 31) << 27 >> 3 >> 24) & 0xff);
                }
                else if ((b1 & QOI_MASK_4) == QOI_COLOR)
                {
                    if ((b1 & 8) != 0)
                        r = data[pos++];
                    if ((b1 & 4) != 0)
                        g = data[pos++];
                    if ((b1 & 2) != 0)
                        b = data[pos++];
                    if ((b1 & 1) != 0)
                        a = data[pos++];
                }

                int indexPos2 = ((r ^ g ^ b ^ a) & 63) << 2;
                index[indexPos2] = r;
                index[indexPos2 + 1] = g;
                index[indexPos2 + 2] = b;
                index[indexPos2 + 3] = a;
            }

            img[imgPos++] = b;
            img[imgPos++] = g;
            img[imgPos++] = r;
            img[imgPos++] = a;
        }

        return img;
    }
}
