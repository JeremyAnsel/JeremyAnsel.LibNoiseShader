using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class BlendFileModule : FileModuleBase
    {
        public IFileModule Input1 { get; set; }

        public IFileModule Input2 { get; set; }

        public IFileModule Control { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            Input1 = context.GetModule(reader.ReadInt32());
            Input2 = context.GetModule(reader.ReadInt32());
            Control = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Input1) == -1)
            {
                Input1?.Write(writer, context);
            }

            if (context.GetModuleIndex(Input2) == -1)
            {
                Input2?.Write(writer, context);
            }

            if (context.GetModuleIndex(Control) == -1)
            {
                Control?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(context.GetModuleIndex(Input1));
            writer.Write(context.GetModuleIndex(Input2));
            writer.Write(context.GetModuleIndex(Control));
        }
    }
}
