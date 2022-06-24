using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class RotatePointFileModule : FileModuleBase
    {
        public float AngleX { get; set; }

        public float AngleY { get; set; }

        public float AngleZ { get; set; }

        public IFileModule Input1 { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            AngleX = reader.ReadSingle();
            AngleY = reader.ReadSingle();
            AngleZ = reader.ReadSingle();
            Input1 = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Input1) == -1)
            {
                Input1?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(AngleX);
            writer.Write(AngleY);
            writer.Write(AngleZ);
            writer.Write(context.GetModuleIndex(Input1));
        }
    }
}
