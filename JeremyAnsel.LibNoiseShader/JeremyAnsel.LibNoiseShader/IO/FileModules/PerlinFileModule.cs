using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class PerlinFileModule : FileModuleBase
    {
        public float Frequency { get; set; }

        public float Lacunarity { get; set; }

        public int OctaveCount { get; set; }

        public float Persistence { get; set; }

        public int SeedOffset { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            Frequency = reader.ReadSingle();
            Lacunarity = reader.ReadSingle();
            OctaveCount = reader.ReadInt32();
            Persistence = reader.ReadSingle();
            SeedOffset = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            base.Write(writer, context);

            writer.Write(Frequency);
            writer.Write(Lacunarity);
            writer.Write(OctaveCount);
            writer.Write(Persistence);
            writer.Write(SeedOffset);
        }
    }
}
