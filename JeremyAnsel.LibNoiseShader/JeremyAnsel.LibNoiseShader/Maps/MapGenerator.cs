using JeremyAnsel.DirectX.D3D11;
using JeremyAnsel.DirectX.GameWindow;
using JeremyAnsel.LibNoiseShader.Builders;
using JeremyAnsel.LibNoiseShader.Renderers;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading.Tasks;

namespace JeremyAnsel.LibNoiseShader.Maps
{
    public static class MapGenerator
    {
        public static ValueMap GenerateValueMap(IBuilder builder, int width, int height)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            float[] buffer = new float[width * height];

            Parallel.ForEach(Partitioner.Create(0, width * height), range =>
            {
                for (int index = range.Item1; index < range.Item2; index++)
                {
                    int y = index / width;
                    int x = index % width;

                    float px = -1.0f + x * (2.0f / width);
                    float py = -1.0f + y * (2.0f / height);

                    buffer[y * width + x] = builder.GetValue(px, py);
                }
            });

            return new ValueMap(width, height, buffer);
        }

        public static ColorMap GenerateColorMapOnCpu(Noise3D noise, IRenderer renderer, int width, int height)
        {
            if (noise is null)
            {
                throw new ArgumentNullException(nameof(noise));
            }

            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            byte[] buffer = new byte[width * height * 4];

            Parallel.ForEach(Partitioner.Create(0, width * height), range =>
            {
                for (int index = range.Item1; index < range.Item2; index++)
                {
                    int y = index / width;
                    int x = index % width;

                    float px = -1.0f + x * (2.0f / width);
                    float py = -1.0f + y * (2.0f / height);

                    Color color = renderer.GetColor(px, py, width, height);

                    int offset = y * width * 4 + x * 4;

                    buffer[offset + 0] = color.B;
                    buffer[offset + 1] = color.G;
                    buffer[offset + 2] = color.R;
                    buffer[offset + 3] = color.A;
                }
            });

            return new ColorMap(width, height, buffer);
        }

        public static ColorMap GenerateColorMapOnGpu(Noise3D noise, IRenderer renderer, int width, int height)
        {
            if (noise is null)
            {
                throw new ArgumentNullException(nameof(noise));
            }

            if (renderer is null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            var options = new DeviceResourcesOptions
            {
                ForceWarp = false,
                UseHighestFeatureLevel = true
            };

            var deviceResources = new RenderTargetDeviceResources((uint)width, (uint)height, D3D11FeatureLevel.FeatureLevel100, options);
            var component = new MapGeneratorGameComponent(noise, renderer);

            byte[] buffer;

            try
            {
                component.CreateDeviceDependentResources(deviceResources);
                component.CreateWindowSizeDependentResources();
                component.Update(null);
                component.Render();
                deviceResources.Present();

                buffer = deviceResources.GetBackBufferContent();
            }
            finally
            {
                component.ReleaseWindowSizeDependentResources();
                component.ReleaseDeviceDependentResources();
                deviceResources.Release();
            }

            return new ColorMap(width, height, buffer);
        }
    }
}
