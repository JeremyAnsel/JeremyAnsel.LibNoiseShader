using System;
using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileRenderers
{
    public abstract class FileRendererBase : IFileRenderer
    {
        public string? Name { get; set; }

        public double PositionX { get; set; }

        public double PositionY { get; set; }

        public virtual void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.AddRenderer(this);

            Name = reader.ReadString();
            PositionX = reader.ReadDouble();
            PositionY = reader.ReadDouble();
        }

        public virtual void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.AddRenderer(this);

            writer.Write(GetType().Name);
            writer.Write(Name ?? string.Empty);
            writer.Write(PositionX);
            writer.Write(PositionY);
        }
    }
}
