using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class DisplaceFileModule : FileModuleBase
    {
        public IFileModule Input1 { get; set; }

        public IFileModule DisplaceX { get; set; }

        public IFileModule DisplaceY { get; set; }

        public IFileModule DisplaceZ { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            Input1 = context.GetModule(reader.ReadInt32());
            DisplaceX = context.GetModule(reader.ReadInt32());
            DisplaceY = context.GetModule(reader.ReadInt32());
            DisplaceZ = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Input1) == -1)
            {
                Input1?.Write(writer, context);
            }

            if (context.GetModuleIndex(DisplaceX) == -1)
            {
                DisplaceX?.Write(writer, context);
            }

            if (context.GetModuleIndex(DisplaceY) == -1)
            {
                DisplaceY?.Write(writer, context);
            }

            if (context.GetModuleIndex(DisplaceZ) == -1)
            {
                DisplaceZ?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(context.GetModuleIndex(Input1));
            writer.Write(context.GetModuleIndex(DisplaceX));
            writer.Write(context.GetModuleIndex(DisplaceY));
            writer.Write(context.GetModuleIndex(DisplaceZ));
        }
    }
}
