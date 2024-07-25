using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class AddModule : ModuleBase
    {
        public AddModule(IModule? module0, IModule? module1)
        {
            this.SetSourceModule(0, module0);
            this.SetSourceModule(1, module1);
        }

        public override int RequiredSourceModuleCount => 2;

        public override float GetValue(float x, float y, float z)
        {
            IModule module0 = this.GetSourceModule(0)!;
            IModule module1 = this.GetSourceModule(1)!;

            return module0.GetValue(x, y, z) + module1.GetValue(x, y, z);
        }

        public override int EmitHlslMaxDepth()
        {
            return 2;
        }

        public override void EmitHlsl(HlslContext context)
        {
            this.GetSourceModule(0)!.EmitHlsl(context);
            this.GetSourceModule(1)!.EmitHlsl(context);
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
            return 2;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            body.AppendTabFormatLine(2, "result = param0 + param1;");
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
