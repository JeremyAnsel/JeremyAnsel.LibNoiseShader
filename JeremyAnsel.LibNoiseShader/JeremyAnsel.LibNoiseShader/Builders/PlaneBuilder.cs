using JeremyAnsel.LibNoiseShader.Models;
using JeremyAnsel.LibNoiseShader.Modules;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Builders
{
    public sealed class PlaneBuilder : BuilderBase
    {
        public PlaneBuilder(IModule source)
            : base(source)
        {
            this.IsSeamless = false;
            this.LowerBoundX = -1.0f;
            this.UpperBoundX = 1.0f;
            this.LowerBoundY = -1.0f;
            this.UpperBoundY = 1.0f;
        }

        public PlaneBuilder(IModule source, bool seamless, float lowerX, float upperX, float lowerY, float upperY)
            : base(source)
        {
            this.IsSeamless = seamless;
            this.LowerBoundX = lowerX;
            this.UpperBoundX = upperX;
            this.LowerBoundY = lowerY;
            this.UpperBoundY = upperY;
        }

        public bool IsSeamless { get; set; }

        public float LowerBoundX { get; set; }

        public float UpperBoundX { get; set; }

        public float LowerBoundY { get; set; }

        public float UpperBoundY { get; set; }

        public override float GetValue(float x, float y)
        {
            float xExtent = this.UpperBoundX - this.LowerBoundX;
            float yExtent = this.UpperBoundY - this.LowerBoundY;

            float xCur = this.LowerBoundX + (x + 1.0f) * xExtent / 2.0f;
            float yCur = this.LowerBoundY + (y + 1.0f) * yExtent / 2.0f;

            float finalValue;

            if (this.IsSeamless)
            {
                float swValue = this.GetSourceModule().GetValue(PlaneModel.GetCoords(xCur, yCur));
                float seValue = this.GetSourceModule().GetValue(PlaneModel.GetCoords(xCur + xExtent, yCur));
                float nwValue = this.GetSourceModule().GetValue(PlaneModel.GetCoords(xCur, yCur + yExtent));
                float neValue = this.GetSourceModule().GetValue(PlaneModel.GetCoords(xCur + xExtent, yCur + yExtent));

                float xBlend = 1.0f - ((xCur - this.LowerBoundX) / xExtent);
                float yBlend = 1.0f - ((yCur - this.LowerBoundY) / yExtent);
                float y0 = Interpolation.Linear(swValue, seValue, xBlend);
                float y1 = Interpolation.Linear(nwValue, neValue, xBlend);

                finalValue = Interpolation.Linear(y0, y1, yBlend);
            }
            else
            {
                finalValue = this.GetSourceModule().GetValue(PlaneModel.GetCoords(xCur, yCur));
            }

            return finalValue;
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule());

            sb.AppendTabFormatLine(context.GetBuilderFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float xExtent = {0} - {1};", this.UpperBoundX, this.LowerBoundX);
            sb.AppendTabFormatLine(1, "float yExtent = {0} - {1};", this.UpperBoundY, this.LowerBoundY);
            sb.AppendTabFormatLine(1, "float xCur = {0} + (x + 1.0f) * xExtent / 2.0f;", this.LowerBoundX);
            sb.AppendTabFormatLine(1, "float yCur = {0} + (y + 1.0f) * yExtent / 2.0f;", this.LowerBoundY);

            if (this.IsSeamless)
            {
                sb.AppendTabFormatLine(1, "float3 swCoords = Model_Plane(xCur, yCur);");
                sb.AppendTabFormatLine(1, "float swValue = {0}( swCoords.x, swCoords.y, swCoords.z );", module0);
                sb.AppendTabFormatLine(1, "float3 seCoords = Model_Plane(xCur + xExtent, yCur);");
                sb.AppendTabFormatLine(1, "float seValue = {0}( seCoords.x, seCoords.y, seCoords.z );", module0);
                sb.AppendTabFormatLine(1, "float3 nwCoords = Model_Plane(xCur, yCur + yExtent);");
                sb.AppendTabFormatLine(1, "float nwValue = {0}( nwCoords.x, nwCoords.y, nwCoords.z );", module0);
                sb.AppendTabFormatLine(1, "float3 neCoords = Model_Plane(xCur + xExtent, yCur + yExtent);");
                sb.AppendTabFormatLine(1, "float neValue = {0}( neCoords.x, neCoords.y, neCoords.z );", module0);
                sb.AppendTabFormatLine(1, "float xBlend = 1.0f - ((xCur - {0}) / xExtent);", this.LowerBoundX);
                sb.AppendTabFormatLine(1, "float yBlend = 1.0f - ((yCur - {0}) / yExtent);", this.LowerBoundY);
                sb.AppendTabFormatLine(1, "float y0 = Interpolation_Linear(swValue, seValue, xBlend);");
                sb.AppendTabFormatLine(1, "float y1 = Interpolation_Linear(nwValue, neValue, xBlend);");
                sb.AppendTabFormatLine(1, "float finalValue = Interpolation_Linear(y0, y1, yBlend);");
            }
            else
            {
                sb.AppendTabFormatLine(1, "float3 coords = Model_Plane(xCur, yCur);");
                sb.AppendTabFormatLine(1, "float finalValue = {0}( coords.x, coords.y, coords.z );", module0);
            }

            sb.AppendTabFormatLine(1, "return finalValue;");
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule());
            string name = context.GetBuilderName(this);
            string type = context.GetBuilderType(this);

            sb.AppendTabFormatLine(
                "{0} {1} = new({2}, {3}, {4}, {5}, {6}, {7});",
                type,
                name,
                module0,
                this.IsSeamless,
                this.LowerBoundX,
                this.UpperBoundX,
                this.LowerBoundY,
                this.UpperBoundY);

            return sb.ToString();
        }
    }
}
