using System.Collections.Generic;
using System.IO;

namespace JeremyAnsel.LibNoiseShader.IO.FileModules
{
    public sealed class CurveFileModule : FileModuleBase
    {
        public IDictionary<float, float> ControlPoints { get; } = new SortedList<float, float>();

        public IFileModule Input1 { get; set; }

        public override void Read(BinaryReader reader, LibNoiseShaderFileContext context)
        {
            base.Read(reader, context);

            int pointsCount = reader.ReadInt32();

            for (int index = 0; index < pointsCount; index++)
            {
                float key = reader.ReadSingle();
                float value = reader.ReadSingle();
                ControlPoints.Add(key, value);
            }

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

            foreach (KeyValuePair<float, float> point in ControlPoints)
            {
                writer.Write(point.Key);
                writer.Write(point.Value);
            }

            writer.Write(context.GetModuleIndex(Input1));
        }
    }
}
