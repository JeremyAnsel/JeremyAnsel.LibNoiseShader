using JeremyAnsel.LibNoiseShader.Models;
using JeremyAnsel.LibNoiseShader.Modules;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Builders
{
    public sealed class CylinderBuilder : BuilderBase
    {
        public CylinderBuilder(IModule source, int seed)
            : base(source, seed)
        {
            this.LowerAngleBound = -180f;
            this.UpperAngleBound = 180f;
            this.LowerHeightBound = -1.0f;
            this.UpperHeightBound = 1.0f;
        }

        public CylinderBuilder(IModule source, int seed, float lowerAngleBound, float upperAngleBound, float lowerHeightBound, float upperHeightBound)
            : base(source, seed)
        {
            this.LowerAngleBound = lowerAngleBound;
            this.UpperAngleBound = upperAngleBound;
            this.LowerHeightBound = lowerHeightBound;
            this.UpperHeightBound = upperHeightBound;
        }

        public float LowerAngleBound { get; set; }

        public float LowerHeightBound { get; set; }

        public float UpperAngleBound { get; set; }

        public float UpperHeightBound { get; set; }

        public override float GetValue(float x, float y)
        {
            float angleExtent = this.UpperAngleBound - this.LowerAngleBound;
            float heightExtent = this.UpperHeightBound - this.LowerHeightBound;

            float curAngle = this.LowerAngleBound + (x + 1.0f) * angleExtent / 2.0f;
            float curHeight = this.LowerHeightBound + (y + 1.0f) * heightExtent / 2.0f;

            return this.GetSourceModule().GetValue(CylinderModel.GetCoords(curAngle, curHeight) + this.Seed);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            //string module0 = context.GetModuleName(this.GetSourceModule());
            string module0 = context.GetBuilderName(this) + "_Module";

            sb.AppendTabFormatLine(context.GetBuilderFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float angleExtent = {0} - {1};", this.UpperAngleBound, this.LowerAngleBound);
            sb.AppendTabFormatLine(1, "float heightExtent = {0} - {1};", this.UpperHeightBound, this.LowerHeightBound);
            sb.AppendTabFormatLine(1, "float curAngle = {0} + (x + 1.0f) * angleExtent / 2.0f;", this.LowerAngleBound);
            sb.AppendTabFormatLine(1, "float curHeight = {0} + (y + 1.0f) * heightExtent / 2.0f;", this.LowerHeightBound);
            sb.AppendTabFormatLine(1, "float3 coords = Model_Cylinder(curAngle, curHeight) + {0};", this.Seed);
            sb.AppendTabFormatLine(1, "return {0}( coords.x, coords.y, coords.z );", module0);
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
                this.Seed,
                this.LowerAngleBound,
                this.UpperAngleBound,
                this.LowerHeightBound,
                this.UpperHeightBound);

            return sb.ToString();
        }
    }
}
