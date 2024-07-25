using JeremyAnsel.LibNoiseShader.IO.FileBuilders;
using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileRenderers
{
    public sealed class NormalFileRenderer : FileRendererBase
    {
        public IFileBuilder? Source { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            Source = context.GetBuilder(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetBuilderIndex(Source) == -1)
            {
                Source?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(context.GetBuilderIndex(Source));
        }
    }
}
