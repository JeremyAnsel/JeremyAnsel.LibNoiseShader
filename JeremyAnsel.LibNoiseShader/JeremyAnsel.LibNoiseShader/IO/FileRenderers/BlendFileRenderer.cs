using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileRenderers
{
    public sealed class BlendFileRenderer : FileRendererBase
    {
        public IFileRenderer Input1 { get; set; }

        public IFileRenderer Input2 { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            Input1 = context.GetRenderer(reader.ReadInt32());
            Input2 = context.GetRenderer(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetRendererIndex(Input1) == -1)
            {
                Input1?.Write(writer, context);
            }

            if (context.GetRendererIndex(Input2) == -1)
            {
                Input2?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(context.GetRendererIndex(Input1));
            writer.Write(context.GetRendererIndex(Input2));
        }
    }
}
