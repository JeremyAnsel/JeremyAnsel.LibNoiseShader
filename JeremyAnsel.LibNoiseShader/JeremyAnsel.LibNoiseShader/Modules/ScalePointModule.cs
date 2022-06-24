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

        public override float GetValue(float x, float y, float z)
        {
            x *= this.ScaleX;
            y *= this.ScaleY;
            z *= this.ScaleZ;

            return this.GetSourceModule(0).GetValue(x, y, z);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return {0}(x * {1}, y * {2}, z * {3});", module0, this.ScaleX, this.ScaleY, this.ScaleZ);
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
            sb.AppendTabFormatLine("{0}.SetScale({1}, {2}, {3});", name, this.ScaleX, this.ScaleY, this.ScaleZ);

            return sb.ToString();
        }

        public void SetScale(float scaleX, float scaleY, float scaleZ)
        {
            this.ScaleX = scaleX;
            this.ScaleY = scaleY;
            this.ScaleZ = scaleZ;
        }
    }
}
