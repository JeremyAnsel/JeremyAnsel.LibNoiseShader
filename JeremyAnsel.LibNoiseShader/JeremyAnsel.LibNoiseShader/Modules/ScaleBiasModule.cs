using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class ScaleBiasModule : ModuleBase
    {
        public ScaleBiasModule(IModule module0)
        {
            this.SetSourceModule(0, module0);

            this.Bias = 0.0f;
            this.Scale = 1.0f;
        }

        public float Bias { get; set; }

        public float Scale { get; set; }

        public override int RequiredSourceModuleCount => 1;

        public override float GetValue(float x, float y, float z)
        {
            return this.GetSourceModule(0).GetValue(x, y, z) * this.Scale + this.Bias;
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return {0}(x, y, z) * {1} + {2};", module0, this.Scale, this.Bias);
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
            sb.AppendTabFormatLine(1, "Bias = {0},", this.Bias);
            sb.AppendTabFormatLine(1, "Scale = {0},", this.Scale);
            sb.AppendTabFormatLine("};");

            return sb.ToString();
        }
    }
}
