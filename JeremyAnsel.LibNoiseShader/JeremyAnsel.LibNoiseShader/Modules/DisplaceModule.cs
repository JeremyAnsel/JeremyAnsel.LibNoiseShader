using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class DisplaceModule : ModuleBase
    {
        public DisplaceModule(IModule module0, IModule displaceXModule, IModule displaceYModule, IModule displaceZModule)
        {
            this.SetSourceModule(0, module0);
            this.SetSourceModule(1, displaceXModule);
            this.SetSourceModule(2, displaceYModule);
            this.SetSourceModule(3, displaceZModule);
        }

        public override int RequiredSourceModuleCount => 4;

        public override float GetValue(float x, float y, float z)
        {
            float displaceX = x + this.GetSourceModule(1).GetValue(x, y, z);
            float displaceY = y + this.GetSourceModule(2).GetValue(x, y, z);
            float displaceZ = z + this.GetSourceModule(3).GetValue(x, y, z);

            return this.GetSourceModule(0).GetValue(displaceX, displaceY, displaceZ);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string displaceXModule = context.GetModuleName(this.GetSourceModule(1));
            string displaceYModule = context.GetModuleName(this.GetSourceModule(2));
            string displaceZModule = context.GetModuleName(this.GetSourceModule(3));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float displaceX = x + {0}(x, y, z);", displaceXModule);
            sb.AppendTabFormatLine(1, "float displaceY = y + {0}(x, y, z);", displaceYModule);
            sb.AppendTabFormatLine(1, "float displaceZ = z + {0}(x, y, z);", displaceZModule);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "return {0}(displaceX, displaceY, displaceZ);", module0);
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string displaceXModule = context.GetModuleName(this.GetSourceModule(1));
            string displaceYModule = context.GetModuleName(this.GetSourceModule(2));
            string displaceZModule = context.GetModuleName(this.GetSourceModule(3));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2}, {3}, {4}, {5});", type, name, module0, displaceXModule, displaceYModule, displaceZModule);

            return sb.ToString();
        }
    }
}
