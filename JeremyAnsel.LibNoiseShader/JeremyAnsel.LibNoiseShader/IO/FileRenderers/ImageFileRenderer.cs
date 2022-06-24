using JeremyAnsel.LibNoiseShader.IO.FileBuilders;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileRenderers
{
    public sealed class ImageFileRenderer : FileRendererBase
    {
        public bool IsWrapEnabled { get; set; }

        public bool IsLightEnabled { get; set; }

        public float LightAzimuth { get; set; }

        public float LightBrightness { get; set; }

        public Color LightColor { get; set; }

        public float LightContrast { get; set; }

        public float LightElevation { get; set; }

        public float LightIntensity { get; set; }

        public IDictionary<float, Color> GradientPoints { get; } = new SortedList<float, Color>();

        public IFileBuilder Source { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            IsWrapEnabled = reader.ReadBoolean();
            IsLightEnabled = reader.ReadBoolean();
            LightAzimuth = reader.ReadSingle();
            LightBrightness = reader.ReadSingle();

            byte lightColorB = reader.ReadByte();
            byte lightColorG = reader.ReadByte();
            byte lightColorR = reader.ReadByte();
            byte lightColorA = reader.ReadByte();
            LightColor = Color.FromArgb(lightColorA, lightColorR, lightColorG, lightColorB);

            LightContrast = reader.ReadSingle();
            LightElevation = reader.ReadSingle();
            LightIntensity = reader.ReadSingle();

            int pointsCount = reader.ReadInt32();

            for (int index = 0; index < pointsCount; index++)
            {
                float key = reader.ReadSingle();

                byte colorB = reader.ReadByte();
                byte colorG = reader.ReadByte();
                byte colorR = reader.ReadByte();
                byte colorA = reader.ReadByte();
                Color color = Color.FromArgb(colorA, colorR, colorG, colorB);
                GradientPoints.Add(key, color);
            }

            Source = context.GetBuilder(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetBuilderIndex(Source) == -1)
            {
                Source?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(IsWrapEnabled);
            writer.Write(IsLightEnabled);
            writer.Write(LightAzimuth);
            writer.Write(LightBrightness);
            Color lightColor = LightColor;
            writer.Write(lightColor.B);
            writer.Write(lightColor.G);
            writer.Write(lightColor.R);
            writer.Write(lightColor.A);
            writer.Write(LightContrast);
            writer.Write(LightElevation);
            writer.Write(LightIntensity);
            writer.Write(GradientPoints.Count);

            foreach (KeyValuePair<float, Color> point in GradientPoints)
            {
                writer.Write(point.Key);
                Color color = point.Value;
                writer.Write(color.B);
                writer.Write(color.G);
                writer.Write(color.R);
                writer.Write(color.A);
            }

            writer.Write(context.GetBuilderIndex(Source));
        }
    }
}
