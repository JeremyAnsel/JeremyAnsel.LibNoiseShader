using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class TranslatePointFileModule : FileModuleBase
    {
        public float TranslateX { get; set; }

        public float TranslateY { get; set; }

        public float TranslateZ { get; set; }

        public IFileModule Input1 { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            TranslateX = reader.ReadSingle();
            TranslateY = reader.ReadSingle();
            TranslateZ = reader.ReadSingle();
            Input1 = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Input1) == -1)
            {
                Input1?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(TranslateX);
            writer.Write(TranslateY);
            writer.Write(TranslateZ);
            writer.Write(context.GetModuleIndex(Input1));
        }
    }
}
