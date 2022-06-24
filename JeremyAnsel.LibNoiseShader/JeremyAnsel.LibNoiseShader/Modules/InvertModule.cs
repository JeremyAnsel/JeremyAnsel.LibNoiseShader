using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class InvertModule : ModuleBase
    {
        public InvertModule(IModule module0)
        {
            this.SetSourceModule(0, module0);
        }

        public override int RequiredSourceModuleCount => 1;

        public override float GetValue(float x, float y, float z)
        {
            return -this.GetSourceModule(0).GetValue(x, y, z);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return -{0}(x, y, z);", module0);
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2});", type, name, module0);

            return sb.ToString();
        }
    }
}
