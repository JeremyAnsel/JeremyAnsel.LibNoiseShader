using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class LineFileModule : FileModuleBase
    {
        public bool Attenuate { get; set; }

        public float StartPointX { get; set; }

        public float StartPointY { get; set; }

        public float StartPointZ { get; set; }

        public float EndPointX { get; set; }

        public float EndPointY { get; set; }

        public float EndPointZ { get; set; }

        public IFileModule Input1 { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            Attenuate = reader.ReadBoolean();
            StartPointX = reader.ReadSingle();
            StartPointY = reader.ReadSingle();
            StartPointZ = reader.ReadSingle();
            EndPointX = reader.ReadSingle();
            EndPointY = reader.ReadSingle();
            EndPointZ = reader.ReadSingle();
            Input1 = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Input1) == -1)
            {
                Input1?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(Attenuate);
            writer.Write(StartPointX);
            writer.Write(StartPointY);
            writer.Write(StartPointZ);
            writer.Write(EndPointX);
            writer.Write(EndPointY);
            writer.Write(EndPointZ);
            writer.Write(context.GetModuleIndex(Input1));
        }
    }
}
