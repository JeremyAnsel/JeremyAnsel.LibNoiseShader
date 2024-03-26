using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class ConstantModule : ModuleBase
    {
        public ConstantModule()
        {
            this.ConstantValue = 0.0f;
        }

        public float ConstantValue { get; set; }

        public override int RequiredSourceModuleCount => 0;

        public override float GetValue(float x, float y, float z)
        {
            return this.ConstantValue;
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
            string key = nameof(ConstantModule);

            header.AppendTabFormatLine("float {0}_ConstantValue = 0.0f;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(ConstantModule);

            body.AppendTabFormatLine(2, "{0}_ConstantValue = {1};", key, this.ConstantValue);
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
            string key = nameof(ConstantModule);

            body.AppendTabFormatLine(2, "result = {0}_ConstantValue;", key);
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new()", type, name);
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "ConstantValue = {0},", this.ConstantValue);
            sb.AppendTabFormatLine("};");

            return sb.ToString();
        }
    }
}
