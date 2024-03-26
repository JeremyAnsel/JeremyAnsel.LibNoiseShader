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

        public override int EmitHlslMaxDepth()
        {
            return 1;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            this.GetSourceModule(0).EmitHlsl(context);
            context.EmitSettings(this);
            context.EmitFunction(this, false);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            string key = nameof(ExponentModule);

            header.AppendTabFormatLine("float {0}_ExponentValue = 1.0f;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(ExponentModule);

            body.AppendTabFormatLine(2, "{0}_ExponentValue = {1};", key, this.ExponentValue);
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
            string key = nameof(ExponentModule);

            body.AppendTabFormatLine(2, "float value = param0 * 0.5f + 0.5f;");
            body.AppendTabFormatLine(2, "result = pow(value * sign(value), {0}_ExponentValue) * 2.0f - 1.0f;", key);
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
