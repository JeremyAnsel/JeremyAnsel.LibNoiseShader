using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class MultiplyModule : ModuleBase
    {
        public MultiplyModule(IModule module0, IModule module1)
        {
            this.SetSourceModule(0, module0);
            this.SetSourceModule(1, module1);
        }

        public override int RequiredSourceModuleCount => 2;

        public override float GetValue(float x, float y, float z)
        {
            return this.GetSourceModule(0).GetValue(x, y, z) * this.GetSourceModule(1).GetValue(x, y, z);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string module1 = context.GetModuleName(this.GetSourceModule(1));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return {0}(x, y, z) * {1}(x, y, z);", module0, module1);
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string module1 = context.GetModuleName(this.GetSourceModule(1));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2}, {3});", type, name, module0, module1);

            return sb.ToString();
        }
    }
}
