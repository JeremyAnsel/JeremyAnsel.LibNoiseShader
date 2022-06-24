using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class ConstantFileModule : FileModuleBase
    {
        public float ConstantValue { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            ConstantValue = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            base.Write(writer, context);

            writer.Write(ConstantValue);
        }
    }
}
