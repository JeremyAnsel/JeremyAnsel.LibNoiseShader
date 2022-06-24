using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class BlendModule : ModuleBase
    {
        public BlendModule(IModule module0, IModule module1, IModule controlModule)
        {
            this.SetSourceModule(0, module0);
            this.SetSourceModule(1, module1);
            this.SetSourceModule(2, controlModule);
        }

        public override int RequiredSourceModuleCount => 3;

        public override float GetValue(float x, float y, float z)
        {
            float v0 = this.GetSourceModule(0).GetValue(x, y, z);
            float v1 = this.GetSourceModule(1).GetValue(x, y, z);
            float alpha = this.GetSourceModule(2).GetValue(x, y, z) * 0.5f + 0.5f;

            return Interpolation.Linear(v0, v1, alpha);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string module1 = context.GetModuleName(this.GetSourceModule(1));
            string controlModule = context.GetModuleName(this.GetSourceModule(2));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float v0 = {0}(x, y, z);", module0);
            sb.AppendTabFormatLine(1, "float v1 = {0}(x, y, z);", module1);
            sb.AppendTabFormatLine(1, "float alpha = {0}(x, y, z) * 0.5f + 0.5f;", controlModule);
            sb.AppendTabFormatLine(1, "return Interpolation_Linear( v0, v1, alpha );");
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string module1 = context.GetModuleName(this.GetSourceModule(1));
            string controlModule = context.GetModuleName(this.GetSourceModule(2));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2}, {3}, {4});", type, name, module0, module1, controlModule);

            return sb.ToString();
        }
    }
}
