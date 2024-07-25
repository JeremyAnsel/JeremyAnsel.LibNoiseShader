using JeremyAnsel.LibNoiseShader.IO.FileModules;
using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileBuilders
{
    public sealed class SphereFileBuilder : FileBuilderBase
    {
        public float SouthLatBound { get; set; }

        public float NorthLatBound { get; set; }

        public float WestLonBound { get; set; }

        public float EastLonBound { get; set; }

        public IFileModule? Source { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            SouthLatBound = reader.ReadSingle();
            NorthLatBound = reader.ReadSingle();
            WestLonBound = reader.ReadSingle();
            EastLonBound = reader.ReadSingle();
            Source = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Source) == -1)
            {
                Source?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(SouthLatBound);
            writer.Write(NorthLatBound);
            writer.Write(WestLonBound);
            writer.Write(EastLonBound);
            writer.Write(context.GetModuleIndex(Source));
        }
    }
}
