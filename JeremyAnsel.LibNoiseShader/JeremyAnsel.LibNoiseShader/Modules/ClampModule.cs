using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class ClampModule : ModuleBase
    {
        public ClampModule(IModule module0)
        {
            this.SetSourceModule(0, module0);

            this.LowerBound = -1.0f;
            this.UpperBound = 1.0f;
        }

        public float LowerBound { get; set; }

        public float UpperBound { get; set; }

        public override int RequiredSourceModuleCount => 1;

        public override float GetValue(float x, float y, float z)
        {
            float value = this.GetSourceModule(0).GetValue(x, y, z);

            value = Math.Max(value, this.LowerBound);
            value = Math.Min(value, this.UpperBound);

            return value;
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "return clamp( {0}(x, y, z), {1}, {2} );", module0, this.LowerBound, this.UpperBound);
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
            sb.AppendTabFormatLine("{0}.SetBounds({1}, {2});", name, this.LowerBound, this.UpperBound);

            return sb.ToString();
        }

        public void SetBounds(float lowerBound, float upperBound)
        {
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
        }
    }
}
