using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class CylinderModule : ModuleBase
    {
        public CylinderModule()
        {
            this.Frequency = 1.0f;
        }

        public float Frequency { get; set; }

        public override int RequiredSourceModuleCount => 0;

        public override float GetValue(float x, float y, float z)
        {
            x *= this.Frequency;
            z *= this.Frequency;

            float distFromCenter = (float)Math.Sqrt((x * x) + (z * z));
            float distFromSmallerSphere = distFromCenter - (float)Math.Floor(distFromCenter);
            float distFromLargerSphere = 1.0f - distFromSmallerSphere;
            float nearestDist = Math.Min(distFromSmallerSphere, distFromLargerSphere);

            // Puts it in the -1.0 to +1.0 range.
            return 1.0f - (nearestDist * 4.0f);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float2 freq = float2(x, z) * {0};", this.Frequency);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float distFromCenter = sqrt( freq.x * freq.x + freq.y * freq.y );");
            sb.AppendTabFormatLine(1, "float distFromSmallerSphere = distFromCenter - floor(distFromCenter);");
            sb.AppendTabFormatLine(1, "float distFromLargerSphere = 1.0f - distFromSmallerSphere;");
            sb.AppendTabFormatLine(1, "float nearestDist = min(distFromSmallerSphere, distFromLargerSphere);");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "return 1.0f - (nearestDist * 4.0f);");
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new()", type, name);
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "Frequency = {0}", this.Frequency);
            sb.AppendTabFormatLine("};");

            return sb.ToString();
        }
    }
}
