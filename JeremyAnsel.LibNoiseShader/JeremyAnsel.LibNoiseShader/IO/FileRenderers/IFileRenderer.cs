using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileRenderers
{
    public interface IFileRenderer
    {
        string? Name { get; set; }

        double PositionX { get; set; }

        double PositionY { get; set; }

        void Read(BinaryReader reader, LibNoiseShaderFileContext context);

        void Write(BinaryWriter writer, LibNoiseShaderFileContext context);
    }
}
