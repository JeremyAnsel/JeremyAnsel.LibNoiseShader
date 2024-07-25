using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class DisplaceModule : ModuleBase
    {
        public DisplaceModule(IModule? module0, IModule? displaceXModule, IModule? displaceYModule, IModule? displaceZModule)
        {
            this.SetSourceModule(0, module0);
            this.SetSourceModule(1, displaceXModule);
            this.SetSourceModule(2, displaceYModule);
            this.SetSourceModule(3, displaceZModule);
        }

        public override int RequiredSourceModuleCount => 4;

        public override float GetValue(float x, float y, float z)
        {
            float displaceX = x + this.GetSourceModule(1)!.GetValue(x, y, z);
            float displaceY = y + this.GetSourceModule(2)!.GetValue(x, y, z);
            float displaceZ = z + this.GetSourceModule(3)!.GetValue(x, y, z);

            return this.GetSourceModule(0)!.GetValue(displaceX, displaceY, displaceZ);
        }

        public override int EmitHlslMaxDepth()
        {
            return 4;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            this.GetSourceModule(1)!.EmitHlsl(context);
            this.GetSourceModule(2)!.EmitHlsl(context);
            this.GetSourceModule(3)!.EmitHlsl(context);
            context.EmitCoords(this, 0, false);
            this.GetSourceModule(0)!.EmitHlsl(context);
            context.EmitFunction(this, true);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
        }

        public override bool HasHlslSettings()
        {
            return false;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
        }

        public override bool HasHlslCoords(int index)
        {
            return true;
        }

        public override void EmitHlslCoords(StringBuilder body, int index)
        {
            body.AppendTabFormatLine(2, "modules_results_index -= 3;");
            body.AppendTabFormatLine(2, "float param0 = modules_results[modules_results_index];");
            body.AppendTabFormatLine(2, "float param1 = modules_results[modules_results_index + 1];");
            body.AppendTabFormatLine(2, "float param2 = modules_results[modules_results_index + 2];");
            body.AppendTabFormatLine(2, "float displaceX = coords.x + param0;");
            body.AppendTabFormatLine(2, "float displaceY = coords.y + param1;");
            body.AppendTabFormatLine(2, "float displaceZ = coords.z + param2;");
            body.AppendTabFormatLine(2, "coords = float3(displaceX, displaceY, displaceZ);");
        }

        public override int GetHlslFunctionParametersCount()
        {
            return 1;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            body.AppendTabFormatLine(2, "return param0;");
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
