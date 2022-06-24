using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class TranslatePointModule : ModuleBase
    {
        public TranslatePointModule(IModule module0)
        {
            this.SetSourceModule(0, module0);

            this.SetTranslate(0.0f, 0.0f, 0.0f);
        }

        public float TranslateX { get; set; }

        public float TranslateY { get; set; }

        public float TranslateZ { get; set; }

        public override int RequiredSourceModuleCount => 1;

        public override float GetValue(float x, float y, float z)
        {
            x += this.TranslateX;
            y += this.TranslateY;
            z += this.TranslateZ;

            return this.GetSourceModule(0).GetValue(x, y, z);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return {0}(x + {1}, y + {2}, z + {3});", module0, this.TranslateX, this.TranslateY, this.TranslateZ);
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
            sb.AppendTabFormatLine("{0}.SetTranslate({1}, {2}, {3});", name, this.TranslateX, this.TranslateY, this.TranslateZ);

            return sb.ToString();
        }

        public void SetTranslate(float translateX, float translateY, float translateZ)
        {
            this.TranslateX = translateX;
            this.TranslateY = translateY;
            this.TranslateZ = translateZ;
        }
    }
}
