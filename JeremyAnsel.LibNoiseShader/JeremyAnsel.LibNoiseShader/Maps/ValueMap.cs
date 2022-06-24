using System;

namespace JeremyAnsel.LibNoiseShader.Maps
{
    public sealed class ValueMap
    {
        public ValueMap(int width, int height, float[] data)
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

            if (data.Length != width * height)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            Width = width;
            Height = height;
            Data = data;
        }

        public int Width { get; }

        public int Height { get; }

        public float[] Data { get; }
    }
}
