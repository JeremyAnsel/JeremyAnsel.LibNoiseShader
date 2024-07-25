using JeremyAnsel.LibNoiseShader.Models;
using JeremyAnsel.LibNoiseShader.Modules;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Builders
{
    public sealed class PlaneBuilder : BuilderBase
    {
        public PlaneBuilder(IModule? source, int seed)
            : base(source, seed)
        {
            this.IsSeamless = false;
            this.LowerBoundX = -1.0f;
            this.UpperBoundX = 1.0f;
            this.LowerBoundY = -1.0f;
            this.UpperBoundY = 1.0f;
        }

        public PlaneBuilder(IModule? source, int seed, bool seamless, float lowerX, float upperX, float lowerY, float upperY)
            : base(source, seed)
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
                float swValue = this.GetSourceModule().GetValue(PlaneModel.GetCoords(xCur, yCur) + this.SeedFloat);
                float seValue = this.GetSourceModule().GetValue(PlaneModel.GetCoords(xCur + xExtent, yCur) + this.SeedFloat);
                float nwValue = this.GetSourceModule().GetValue(PlaneModel.GetCoords(xCur, yCur + yExtent) + this.SeedFloat);
                float neValue = this.GetSourceModule().GetValue(PlaneModel.GetCoords(xCur + xExtent, yCur + yExtent) + this.SeedFloat);

                float xBlend = 1.0f - ((xCur - this.LowerBoundX) / xExtent);
                float yBlend = 1.0f - ((yCur - this.LowerBoundY) / yExtent);
                float y0 = Interpolation.Linear(swValue, seValue, xBlend);
                float y1 = Interpolation.Linear(nwValue, neValue, xBlend);

                finalValue = Interpolation.Linear(y0, y1, yBlend);
            }
            else
            {
                finalValue = this.GetSourceModule().GetValue(PlaneModel.GetCoords(xCur, yCur) + this.SeedFloat);
            }

            return finalValue;
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            //string module0 = context.GetModuleName(this.GetSourceModule());
            string module0 = context.GetBuilderName(this) + "_Module";

            sb.AppendTabFormatLine(context.GetBuilderFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float xExtent = {0} - {1};", this.UpperBoundX, this.LowerBoundX);
            sb.AppendTabFormatLine(1, "float yExtent = {0} - {1};", this.UpperBoundY, this.LowerBoundY);
            sb.AppendTabFormatLine(1, "float xCur = {0} + (x + 1.0f) * xExtent / 2.0f;", this.LowerBoundX);
            sb.AppendTabFormatLine(1, "float yCur = {0} + (y + 1.0f) * yExtent / 2.0f;", this.LowerBoundY);

            if (this.IsSeamless)
            {
                sb.AppendTabFormatLine(1, "static const int modules_instructions_count = 8;");
                sb.AppendTabFormatLine(1, "int modules_instructions[] = {{ 1, 0, 2, 0, 3, 0, 4, 0 }};");
                sb.AppendTabFormatLine(1, "float3 coords = (float3)0;");
                sb.AppendTabFormatLine(1, "float params[4] = {{ 0, 0, 0, 0 }};");
                sb.AppendTabFormatLine(1, "[fastopt]");
                sb.AppendTabFormatLine(1, "for (int instruction_index = 0; instruction_index < modules_instructions_count; instruction_index++)");
                sb.AppendTabFormatLine(1, "{");
                sb.AppendTabFormatLine(2, "int instruction = modules_instructions[instruction_index];");
                sb.AppendTabFormatLine(2, "[call]");
                sb.AppendTabFormatLine(2, "switch (instruction)");
                sb.AppendTabFormatLine(2, "{");
                sb.AppendTabFormatLine(2, "case 0:");
                sb.AppendTabFormatLine(3, "params[instruction_index/2] = {0}(coords);", module0);
                sb.AppendTabFormatLine(3, "break;");
                sb.AppendTabFormatLine(2, "case 1:");
                sb.AppendTabFormatLine(3, "coords = Model_Plane(xCur, yCur) + {0};", this.Seed);
                sb.AppendTabFormatLine(3, "break;");
                sb.AppendTabFormatLine(2, "case 2:");
                sb.AppendTabFormatLine(3, "coords = Model_Plane(xCur + xExtent, yCur) + {0};", this.Seed);
                sb.AppendTabFormatLine(3, "break;");
                sb.AppendTabFormatLine(2, "case 3:");
                sb.AppendTabFormatLine(3, "coords = Model_Plane(xCur, yCur + yExtent) + {0};", this.Seed);
                sb.AppendTabFormatLine(3, "break;");
                sb.AppendTabFormatLine(2, "case 4:");
                sb.AppendTabFormatLine(3, "coords = Model_Plane(xCur + xExtent, yCur + yExtent) + {0};", this.Seed);
                sb.AppendTabFormatLine(3, "break;");
                sb.AppendTabFormatLine(2, "");
                sb.AppendTabFormatLine(2, "}");
                sb.AppendTabFormatLine(1, "}");
                sb.AppendTabFormatLine(1, "float xBlend = 1.0f - ((xCur - {0}) / xExtent);", this.LowerBoundX);
                sb.AppendTabFormatLine(1, "float yBlend = 1.0f - ((yCur - {0}) / yExtent);", this.LowerBoundY);
                sb.AppendTabFormatLine(1, "float y0 = Interpolation_Linear(params[0], params[1], xBlend);");
                sb.AppendTabFormatLine(1, "float y1 = Interpolation_Linear(params[2], params[3], xBlend);");
                sb.AppendTabFormatLine(1, "float finalValue = Interpolation_Linear(y0, y1, yBlend);");
            }
            else
            {
                sb.AppendTabFormatLine(1, "float3 coords = Model_Plane(xCur, yCur) + {0};", this.Seed);
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
                "{0} {1} = new({2}, {3}, {4}, {5}, {6}, {7}, {8});",
                type,
                name,
                module0,
                this.Seed,
                this.IsSeamless,
                this.LowerBoundX,
                this.UpperBoundX,
                this.LowerBoundY,
                this.UpperBoundY);

            return sb.ToString();
        }
    }
}
