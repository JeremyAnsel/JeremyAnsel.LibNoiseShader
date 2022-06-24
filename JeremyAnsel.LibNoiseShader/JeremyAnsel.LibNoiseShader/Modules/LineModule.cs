using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class LineModule : ModuleBase
    {
        public LineModule(IModule module0)
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

            float value = this.GetSourceModule(0).GetValue(newx, newy, newz);

            if (this.Attenuate)
            {
                value = p * (1.0f - p) * 4 * value;
            }

            return value;
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float p = x;");
            sb.AppendTabFormatLine(1, "float3 endPoint = float3({0}, {1}, {2});", this.EndPointX, this.EndPointY, this.EndPointZ);
            sb.AppendTabFormatLine(1, "float3 startPoint = float3({0}, {1}, {2});", this.StartPointX, this.StartPointY, this.StartPointZ);
            sb.AppendTabFormatLine(1, "float3 newXYZ = (endPoint - startPoint) * p + startPoint;");
            sb.AppendTabFormatLine(1, "float value = {0}(newXYZ.x, newXYZ.y, newXYZ.z);", module0);

            if (this.Attenuate)
            {
                sb.AppendTabFormatLine(1, "value = p * (1.0f - p) * 4 * value;");
            }

            sb.AppendTabFormatLine(1, "return value;");
            sb.AppendTabFormatLine("}");

            return sb.ToString();
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
