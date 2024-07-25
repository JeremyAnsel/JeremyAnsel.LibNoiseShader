using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class BlendModule : ModuleBase
    {
        public BlendModule(IModule? module0, IModule? module1, IModule? controlModule)
        {
            this.SetSourceModule(0, module0);
            this.SetSourceModule(1, module1);
            this.SetSourceModule(2, controlModule);
        }

        public override int RequiredSourceModuleCount => 3;

        public override float GetValue(float x, float y, float z)
        {
            float v0 = this.GetSourceModule(0)!.GetValue(x, y, z);
            float v1 = this.GetSourceModule(1)!.GetValue(x, y, z);
            float alpha = this.GetSourceModule(2)!.GetValue(x, y, z) * 0.5f + 0.5f;

            return Interpolation.Linear(v0, v1, alpha);
        }

        public override int EmitHlslMaxDepth()
        {
            return 3;
        }

        public override void EmitHlsl(HlslContext context)
        {
            this.GetSourceModule(0)!.EmitHlsl(context);
            this.GetSourceModule(1)!.EmitHlsl(context);
            this.GetSourceModule(2)!.EmitHlsl(context);
            context.EmitFunction(this, false);
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
            return false;
        }

        public override void EmitHlslCoords(StringBuilder body, int index)
        {
        }

        public override int GetHlslFunctionParametersCount()
        {
            return 3;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            body.AppendTabFormatLine(2, "float alpha = param2 * 0.5f + 0.5f;");
            body.AppendTabFormatLine(2, "result = Interpolation_Linear( param0, param1, alpha );");
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
