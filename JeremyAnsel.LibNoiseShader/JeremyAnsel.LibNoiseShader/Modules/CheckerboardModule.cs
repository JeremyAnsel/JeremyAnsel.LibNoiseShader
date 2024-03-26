using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class CheckerboardModule : ModuleBase
    {
        public CheckerboardModule()
        {
        }

        public override int RequiredSourceModuleCount => 0;

        public override float GetValue(float x, float y, float z)
        {
            int ix = (int)Math.Floor(x);
            int iy = (int)Math.Floor(y);
            int iz = (int)Math.Floor(z);

            return ((ix ^ iy ^ iz) & 1) != 0 ? -1.0f : 1.0f;
        }

        public override int EmitHlslMaxDepth()
        {
            return 0;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitFunction(this, false);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
        }

        public override bool HasHlslSettings()
        {
            return false;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
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
            body.AppendTabFormatLine(2, "int3 i = (int3)floor(p);");
            body.AppendTabFormatLine(2, "result = ((i.x ^ i.y ^ i.z) & 1) != 0 ? -1.0f : 1.0f;");
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new();", type, name);

            return sb.ToString();
        }
    }
}
