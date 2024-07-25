using JeremyAnsel.LibNoiseShader.IO.FileModules;
using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileBuilders
{
    public sealed class PlaneFileBuilder : FileBuilderBase
    {
        public bool IsSeamless { get; set; }

        public float LowerBoundX { get; set; }

        public float UpperBoundX { get; set; }

        public float LowerBoundY { get; set; }

        public float UpperBoundY { get; set; }

        public IFileModule? Source { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            IsSeamless = reader.ReadBoolean();
            LowerBoundX = reader.ReadSingle();
            UpperBoundX = reader.ReadSingle();
            LowerBoundY = reader.ReadSingle();
            UpperBoundY = reader.ReadSingle();
            Source = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Source) == -1)
            {
                Source?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(IsSeamless);
            writer.Write(LowerBoundX);
            writer.Write(UpperBoundX);
            writer.Write(LowerBoundY);
            writer.Write(UpperBoundY);
            writer.Write(context.GetModuleIndex(Source));
        }
    }
}
