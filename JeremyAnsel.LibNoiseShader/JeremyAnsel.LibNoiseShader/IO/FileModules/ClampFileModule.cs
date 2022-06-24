using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class ClampFileModule : FileModuleBase
    {
        public float LowerBound { get; set; }

        public float UpperBound { get; set; }

        public IFileModule Input1 { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            LowerBound = reader.ReadSingle();
            UpperBound = reader.ReadSingle();
            Input1 = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Input1) == -1)
            {
                Input1?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(LowerBound);
            writer.Write(UpperBound);
            writer.Write(context.GetModuleIndex(Input1));
        }
    }
}
