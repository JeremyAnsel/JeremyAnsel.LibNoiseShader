using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class VoronoiFileModule : FileModuleBase
    {
        public bool IsDistanceApplied { get; set; }

        public float Displacement { get; set; }

        public float Frequency { get; set; }

        public int SeedOffset { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            IsDistanceApplied = reader.ReadBoolean();
            Displacement = reader.ReadSingle();
            Frequency = reader.ReadSingle();
            SeedOffset = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            base.Write(writer, context);

            writer.Write(IsDistanceApplied);
            writer.Write(Displacement);
            writer.Write(Frequency);
            writer.Write(SeedOffset);
        }
    }
}
