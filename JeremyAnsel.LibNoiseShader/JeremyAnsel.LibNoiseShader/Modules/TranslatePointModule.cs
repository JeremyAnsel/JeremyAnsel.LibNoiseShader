using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class TranslatePointModule : ModuleBase
    {
        public TranslatePointModule(IModule? module0)
        {
            this.SetSourceModule(0, module0);

            this.SetTranslate(0.0f, 0.0f, 0.0f);
        }

        public float TranslateX { get; set; }

        public float TranslateY { get; set; }

        public float TranslateZ { get; set; }

        public override int RequiredSourceModuleCount => 1;

        public void SetTranslate(float translateX, float translateY, float translateZ)
        {
            this.TranslateX = translateX;
            this.TranslateY = translateY;
            this.TranslateZ = translateZ;
        }

        public override float GetValue(float x, float y, float z)
        {
            x += this.TranslateX;
            y += this.TranslateY;
            z += this.TranslateZ;

            return this.GetSourceModule(0)!.GetValue(x, y, z);
        }

        public override int EmitHlslMaxDepth()
        {
            return 1;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            context.EmitCoords(this, 0, false);
            this.GetSourceModule(0)!.EmitHlsl(context);
            context.EmitSettings(this);
            context.EmitFunction(this, true);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            string key = nameof(TranslatePointModule);

            header.AppendTabFormatLine("float3 {0}_Translate = float3(0.0f, 0.0f, 0.0f);", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(TranslatePointModule);

            body.AppendTabFormatLine(2, "{0}_Translate = float3({1}, {2}, {3});", key, this.TranslateX, this.TranslateY, this.TranslateZ);
        }

        public override bool HasHlslCoords(int index)
        {
            return true;
        }

        public override void EmitHlslCoords(StringBuilder body, int index)
        {
            body.AppendTabFormatLine(2, "coords = coords + float3({0}, {1}, {2});", this.TranslateX, this.TranslateY, this.TranslateZ);
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
            sb.AppendTabFormatLine("{0}.SetTranslate({1}, {2}, {3});", name, this.TranslateX, this.TranslateY, this.TranslateZ);

            return sb.ToString();
        }
    }
}
