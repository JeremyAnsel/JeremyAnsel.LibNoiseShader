using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace JeremyAnsel.LibNoiseShader.Maps
{
    public sealed class ColorMap
    {
        public ColorMap(int width, int height, byte[]? data)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length != width * height * 4)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            Width = width;
            Height = height;
            Data = data;
        }

        public int Width { get; }

        public int Height { get; }

        public byte[] Data { get; }

        public void SaveBitmap(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            ImageFormat format = Path.GetExtension(filename).ToLower() switch
            {
                ".bmp" => ImageFormat.Bmp,
                ".png" => ImageFormat.Png,
                ".jpg" => ImageFormat.Jpeg,
                _ => throw new NotSupportedException(),
            };
            var handle = GCHandle.Alloc(Data, GCHandleType.Pinned);

            try
            {
                using var bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppArgb, handle.AddrOfPinnedObject());
                bitmap.Save(filename, format);
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
