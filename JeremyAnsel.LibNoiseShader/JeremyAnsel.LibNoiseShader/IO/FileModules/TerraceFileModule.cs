using System.Collections.Generic;
using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class TerraceFileModule : FileModuleBase
    {
        public ICollection<float> ControlPoints { get; } = new SortedSet<float>();

        public bool IsInverted { get; set; }

        public IFileModule Input1 { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            int pointsCount = reader.ReadInt32();

            for (int index = 0; index < pointsCount; index++)
            {
                float value = reader.ReadSingle();
                ControlPoints.Add(value);
            }

            IsInverted = reader.ReadBoolean();
            Input1 = context.GetModule(reader.ReadInt32());
        }

        public override void Write(BinaryWriter writer, LibNoiseShaderFileContext context)
        {
            if (context.GetModuleIndex(Input1) == -1)
            {
                Input1?.Write(writer, context);
            }

            base.Write(writer, context);

            writer.Write(ControlPoints.Count);

            foreach (float point in ControlPoints)
            {
                writer.Write(point);
            }

            writer.Write(IsInverted);
            writer.Write(context.GetModuleIndex(Input1));
        }
    }
}
