using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class TurbulenceFileModule : FileModuleBase
    {
        public float Frequency { get; set; }

        public float Power { get; set; }

        public int Roughness { get; set; }

        public int SeedOffset { get; set; }

        public IFileModule Input1 { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            Frequency = reader.ReadSingle();
            Power = reader.ReadSingle();
            Roughness = reader.ReadInt32();
            SeedOffset = reader.ReadInt32();
            Input1 = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Input1) == -1)
            {
                Input1?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(Frequency);
            writer.Write(Power);
            writer.Write(Roughness);
            writer.Write(SeedOffset);
            writer.Write(context.GetModuleIndex(Input1));
        }
    }
}
