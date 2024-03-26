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

        public override int EmitHlslMaxDepth()
        {
            return 0;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            context.EmitSettings(this);
            context.EmitFunction(this, false);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            string key = nameof(CylinderModule);

            header.AppendTabFormatLine("float {0}_Frequency = 1.0f;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(CylinderModule);

            body.AppendTabFormatLine(2, "{0}_Frequency = {1};", key, this.Frequency);
        }

        public override bool HasHlslCoords(int index)
        {
            return false;
        }

        public override void EmitHlslCoords(StringBuilder body, int index)
        {
        }

        public override int GetHlslFunctionParametersCount()
        {
            return 0;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            string key = nameof(CylinderModule);

            body.AppendTabFormatLine(2, "float2 freq = p.xz * {0}_Frequency;", key);
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "float distFromCenter = sqrt( freq.x * freq.x + freq.y * freq.y );");
            body.AppendTabFormatLine(2, "float distFromSmallerSphere = distFromCenter - floor(distFromCenter);");
            body.AppendTabFormatLine(2, "float distFromLargerSphere = 1.0f - distFromSmallerSphere;");
            body.AppendTabFormatLine(2, "float nearestDist = min(distFromSmallerSphere, distFromLargerSphere);");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "result = 1.0f - (nearestDist * 4.0f);");
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
