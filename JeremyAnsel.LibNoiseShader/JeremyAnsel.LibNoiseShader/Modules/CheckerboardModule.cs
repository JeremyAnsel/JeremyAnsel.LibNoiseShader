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

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "int3 i = (int3)floor(float3(x, y, z));");
            sb.AppendTabFormatLine(1, "return ((i.x ^ i.y ^ i.z) & 1) != 0 ? -1.0f : 1.0f;");
            sb.AppendTabFormatLine("}");

            return sb.ToString();
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
