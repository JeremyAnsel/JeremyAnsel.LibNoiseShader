using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class RidgedMultiFileModule : FileModuleBase
    {
        public float Frequency { get; set; }

        public float Lacunarity { get; set; }

        public float Offset { get; set; }

        public float Gain { get; set; }

        public float Exponent { get; set; }

        public int OctaveCount { get; set; }

        public int SeedOffset { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            Frequency = reader.ReadSingle();
            Lacunarity = reader.ReadSingle();
            Offset = reader.ReadSingle();
            Gain = reader.ReadSingle();
            Exponent = reader.ReadSingle();
            OctaveCount = reader.ReadInt32();
            SeedOffset = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            base.Write(writer, context);

            writer.Write(Frequency);
            writer.Write(Lacunarity);
            writer.Write(Offset);
            writer.Write(Gain);
            writer.Write(Exponent);
            writer.Write(OctaveCount);
            writer.Write(SeedOffset);
        }
    }
}
