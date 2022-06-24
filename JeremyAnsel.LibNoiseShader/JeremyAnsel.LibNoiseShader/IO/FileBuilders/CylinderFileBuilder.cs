using JeremyAnsel.LibNoiseShader.IO.FileModules;
using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileBuilders
{
    public sealed class CylinderFileBuilder : FileBuilderBase
    {
        public float LowerAngleBound { get; set; }

        public float LowerHeightBound { get; set; }

        public float UpperAngleBound { get; set; }

        public float UpperHeightBound { get; set; }

        public IFileModule Source { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            LowerAngleBound = reader.ReadSingle();
            LowerHeightBound = reader.ReadSingle();
            UpperAngleBound = reader.ReadSingle();
            UpperHeightBound = reader.ReadSingle();
            Source = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Source) == -1)
            {
                Source?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(LowerAngleBound);
            writer.Write(LowerHeightBound);
            writer.Write(UpperAngleBound);
            writer.Write(UpperHeightBound);
            writer.Write(context.GetModuleIndex(Source));
        }
    }
}
