using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class CheckerboardFileModule : FileModuleBase
    {
        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            base.Write(writer, context);
        }
    }
}
