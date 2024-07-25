using System.Reflection;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class LineModule : ModuleBase
    {
        public LineModule(IModule? module0)
        {
            this.SetSourceModule(0, module0);

            this.Attenuate = true;
            this.StartPointX = 0.0f;
            this.StartPointY = 0.0f;
            this.StartPointZ = 0.0f;
            this.EndPointX = 1.0f;
            this.EndPointY = 1.0f;
            this.EndPointZ = 1.0f;
        }

        public bool Attenuate { get; set; }

        public float StartPointX { get; set; }

        public float StartPointY { get; set; }

        public float StartPointZ { get; set; }

        public float EndPointX { get; set; }

        public float EndPointY { get; set; }

        public float EndPointZ { get; set; }

        public override int RequiredSourceModuleCount => 1;

        public override float GetValue(float x, float y, float z)
        {
            float p = x;

            float newx = (this.EndPointX - this.StartPointX) * p + this.StartPointX;
            float newy = (this.EndPointY - this.StartPointY) * p + this.StartPointY;
            float newz = (this.EndPointZ - this.StartPointZ) * p + this.StartPointZ;

            float value = this.GetSourceModule(0)!.GetValue(newx, newy, newz);

            if (this.Attenuate)
            {
                value = p * (1.0f - p) * 4 * value;
            }

            return value;
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
            string key = nameof(LineModule);

            header.AppendTabFormatLine("bool {0}_Attenuate = true;", key);
            header.AppendTabFormatLine("float3 {0}_StartPoint = float3(0.0f, 0.0f, 0.0f);", key);
            header.AppendTabFormatLine("float3 {0}_EndPoint = float3(1.0f, 1.0f, 1.0f);", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(LineModule);

            body.AppendTabFormatLine(2, "{0}_Attenuate = {1};", key, this.Attenuate ? "true" : "false");
            body.AppendTabFormatLine(2, "{0}_StartPoint = float3({1}, {2}, {3});", key, this.StartPointX, this.StartPointY, this.StartPointZ);
            body.AppendTabFormatLine(2, "{0}_EndPoint = float3({1}, {2}, {3});", key, this.EndPointX, this.EndPointY, this.EndPointZ);
        }

        public override bool HasHlslCoords(int index)
        {
            return true;
        }

        public override void EmitHlslCoords(StringBuilder body, int index)
        {
            body.AppendTabFormatLine(2, "float point = coords.x;");
            body.AppendTabFormatLine(2, "float3 endPoint = float3({0}, {1}, {2});", this.EndPointX, this.EndPointY, this.EndPointZ);
            body.AppendTabFormatLine(2, "float3 startPoint = float3({0}, {1}, {2});", this.StartPointX, this.StartPointY, this.StartPointZ);
            body.AppendTabFormatLine(2, "coords = (endPoint - startPoint) * point + startPoint;");
        }

        public override int GetHlslFunctionParametersCount()
        {
            return 1;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            string key = nameof(LineModule);

            body.AppendTabFormatLine(2, "float value = param0;");
            body.AppendTabFormatLine(2, "if ({0}_Attenuate)", key);
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "value = p.x * (1.0f - p.x) * 4 * value;");
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine(2, "result = value;");
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new({2})", type, name, module0);
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "Attenuate = {0},", this.Attenuate);
            sb.AppendTabFormatLine("};");
            sb.AppendTabFormatLine("{0}.SetStartPoint({1}, {2}, {3});", name, this.StartPointX, this.StartPointY, this.StartPointZ);
            sb.AppendTabFormatLine("{0}.SetEndPoint({1}, {2}, {3});", name, this.EndPointX, this.EndPointY, this.EndPointZ);

            return sb.ToString();
        }

        public void SetStartPoint(float x, float y, float z)
        {
            this.StartPointX = x;
            this.StartPointY = y;
            this.StartPointZ = z;
        }

        public void SetEndPoint(float x, float y, float z)
        {
            this.EndPointX = x;
            this.EndPointY = y;
            this.EndPointZ = z;
        }
    }
}
