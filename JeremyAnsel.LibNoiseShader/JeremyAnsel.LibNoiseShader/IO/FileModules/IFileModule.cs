using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public interface IFileModule
    {
        string? Name { get; set; }

        double PositionX { get; set; }

        double PositionY { get; set; }

        void Read(BinaryReader reader, LibNoiseShaderFileContext context);

        void Write(BinaryWriter writer, LibNoiseShaderFileContext context);
    }
}
