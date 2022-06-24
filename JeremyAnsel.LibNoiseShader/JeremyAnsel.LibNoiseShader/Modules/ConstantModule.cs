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

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return {0};", this.ConstantValue);
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
            sb.AppendTabFormatLine(1, "ConstantValue = {0},", this.ConstantValue);
            sb.AppendTabFormatLine("};");

            return sb.ToString();
        }
    }
}
