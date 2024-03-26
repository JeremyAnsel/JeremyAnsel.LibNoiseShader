using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class ScalePointModule : ModuleBase
    {
        public ScalePointModule(IModule module0)
        {
            this.SetSourceModule(0, module0);

            this.SetScale(1.0f, 1.0f, 1.0f);
        }

        public float ScaleX { get; set; }

        public float ScaleY { get; set; }

        public float ScaleZ { get; set; }

        public override int RequiredSourceModuleCount => 1;

        public void SetScale(float scaleX, float scaleY, float scaleZ)
        {
            this.ScaleX = scaleX;
            this.ScaleY = scaleY;
            this.ScaleZ = scaleZ;
        }

        public override float GetValue(float x, float y, float z)
        {
            x *= this.ScaleX;
            y *= this.ScaleY;
            z *= this.ScaleZ;

            return this.GetSourceModule(0).GetValue(x, y, z);
        }

        public override int EmitHlslMaxDepth()
        {
            return 1;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            context.EmitCoords(this, 0, false);
            this.GetSourceModule(0).EmitHlsl(context);
            context.EmitSettings(this);
            context.EmitFunction(this, true);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            string key = nameof(ScalePointModule);

            header.AppendTabFormatLine("float3 {0}_Scale = float3(1.0f, 1.0f, 1.0f);", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(ScalePointModule);

            body.AppendTabFormatLine(2, "{0}_Scale = float3({1}, {2}, {3});", key, this.ScaleX, this.ScaleY, this.ScaleZ);
        }

        public override bool HasHlslCoords(int index)
        {
            return true;
        }

        public override void EmitHlslCoords(StringBuilder body, int index)
        {
            body.AppendTabFormatLine(2, "coords = coords * float3({0}, {1}, {2});", this.ScaleX, this.ScaleY, this.ScaleZ);
        }

        public override int GetHlslFunctionParametersCount()
        {
            return 1;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            body.AppendTabFormatLine(2, "result = param0;");
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2});", type, name, module0);
            sb.AppendTabFormatLine("{0}.SetScale({1}, {2}, {3});", name, this.ScaleX, this.ScaleY, this.ScaleZ);

            return sb.ToString();
        }
    }
}
