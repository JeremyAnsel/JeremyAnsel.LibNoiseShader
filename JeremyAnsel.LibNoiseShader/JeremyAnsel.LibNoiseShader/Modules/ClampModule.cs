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

        public override int EmitHlslMaxDepth()
        {
            return 1;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            this.GetSourceModule(0).EmitHlsl(context);
            context.EmitSettings(this);
            context.EmitFunction(this, false);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            string key = nameof(ClampModule);

            header.AppendTabFormatLine("float {0}_LowerBound = -1.0f;", key);
            header.AppendTabFormatLine("float {0}_UpperBound = 1.0f;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(ClampModule);

            body.AppendTabFormatLine(2, "{0}_LowerBound = {1};", key, this.LowerBound);
            body.AppendTabFormatLine(2, "{0}_UpperBound = {1};", key, this.UpperBound);
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
            return 1;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            string key = nameof(ClampModule);

            body.AppendTabFormatLine(2, "result = clamp( param0, {0}_LowerBound, {0}_UpperBound );", key);
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
