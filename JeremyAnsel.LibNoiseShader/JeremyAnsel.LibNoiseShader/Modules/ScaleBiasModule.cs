using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class ScaleBiasModule : ModuleBase
    {
        public ScaleBiasModule(IModule? module0)
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
            return this.GetSourceModule(0)!.GetValue(x, y, z) * this.Scale + this.Bias;
        }

        public override int EmitHlslMaxDepth()
        {
            return 1;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            this.GetSourceModule(0)!.EmitHlsl(context);
            context.EmitSettings(this);
            context.EmitFunction(this, false);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            string key = nameof(ScaleBiasModule);

            header.AppendTabFormatLine("float {0}_Bias = 0.0f;", key);
            header.AppendTabFormatLine("float {0}_Scale = 1.0f;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(ScaleBiasModule);

            body.AppendTabFormatLine(2, "{0}_Bias = {1};", key, this.Bias);
            body.AppendTabFormatLine(2, "{0}_Scale = {1};", key, this.Scale);
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
            return 1;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            string key = nameof(ScaleBiasModule);

            body.AppendTabFormatLine(2, "result = param0 * {0}_Scale + {0}_Bias;", key);
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
