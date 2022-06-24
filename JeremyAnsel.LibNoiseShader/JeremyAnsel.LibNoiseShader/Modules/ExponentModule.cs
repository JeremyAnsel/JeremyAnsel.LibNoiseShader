using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class ExponentModule : ModuleBase
    {
        public ExponentModule(IModule module0)
        {
            this.SetSourceModule(0, module0);

            this.ExponentValue = 1.0f;
        }

        public float ExponentValue { get; set; }

        public override int RequiredSourceModuleCount => 1;

        public override float GetValue(float x, float y, float z)
        {
            float value = this.GetSourceModule(0).GetValue(x, y, z);

            return (float)Math.Pow(Math.Abs(value * 0.5f + 0.5f), this.ExponentValue) * 2.0f - 1.0f;
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float value = {0}(x, y, z) * 0.5f + 0.5f;", module0);
            sb.AppendTabFormatLine(1, "return pow(value * sign(value), {0}) * 2.0f - 1.0f;", this.ExponentValue);
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2})", type, name, module0);
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "ExponentValue = {0},", this.ExponentValue);
            sb.AppendTabFormatLine("};");

            return sb.ToString();
        }
    }
}
