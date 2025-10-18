using JeremyAnsel.DirectX.WinCodec;
using System;
using System.IO;

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

            WicPixelFormatGuid format = WicGuids.GUID_WICPixelFormat32bppBGRA;

            var container = System.IO.Path.GetExtension(filename).ToUpperInvariant() switch
            {
                ".BMP" => WicGuids.GUID_ContainerFormatBmp,
                ".PNG" => WicGuids.GUID_ContainerFormatPng,
                ".JPG" or ".JPEG" => WicGuids.GUID_ContainerFormatJpeg,
                _ => throw new InvalidOperationException(),
            };

            using var factory = WicImagingFactory.Create();
            using var encoder = factory.CreateEncoder(container);
            using var stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            encoder.Initialize(stream, WicBitmapEncoderCacheOption.WICBitmapEncoderNoCache);
            using var frame = encoder.CreateNewFrame();
            frame.Initialize();
            frame.SetSize((uint)Width, (uint)Height);
            frame.SetPixelFormat(ref format);

            using var bitmap = factory.CreateBitmapFromMemory(
                (uint)Width,
                (uint)Height,
                WicGuids.GUID_WICPixelFormat32bppBGRA,
                (uint)(Width * 4),
                Data);
            using var bitmap2 = WicImagingFactory.ConvertBitmapSource(format, bitmap)!;
            frame.WriteSource(bitmap2, null);
            frame.Commit();
            encoder.Commit();
        }
    }
}
