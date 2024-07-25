using JeremyAnsel.LibNoiseShader.Models;
using JeremyAnsel.LibNoiseShader.Modules;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Builders
{
    public sealed class SphereBuilder : BuilderBase
    {
        public SphereBuilder(IModule? source, int seed)
            : base(source, seed)
        {
            this.SouthLatBound = -90.0f;
            this.NorthLatBound = 90.0f;
            this.WestLonBound = -180.0f;
            this.EastLonBound = 180.0f;
        }

        public SphereBuilder(IModule? source, int seed, float southLatBound, float northLatBound, float westLonBound, float eastLonBound)
            : base(source, seed)
        {
            this.SouthLatBound = southLatBound;
            this.NorthLatBound = northLatBound;
            this.WestLonBound = westLonBound;
            this.EastLonBound = eastLonBound;
        }

        public float SouthLatBound { get; set; }

        public float NorthLatBound { get; set; }

        public float WestLonBound { get; set; }

        public float EastLonBound { get; set; }

        public override float GetValue(float x, float y)
        {
            float latExtent = this.NorthLatBound - this.SouthLatBound;
            float lonExtent = this.EastLonBound - this.WestLonBound;

            float curLat = this.SouthLatBound + (y + 1.0f) * latExtent / 2.0f;
            float curLon = this.WestLonBound + (x + 1.0f) * lonExtent / 2.0f;

            return this.GetSourceModule().GetValue(SphereModel.GetCoords(curLat, curLon) + this.SeedFloat);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            //string module0 = context.GetModuleName(this.GetSourceModule());
            string module0 = context.GetBuilderName(this) + "_Module";

            sb.AppendTabFormatLine(context.GetBuilderFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float latExtent = {0} - {1};", this.NorthLatBound, this.SouthLatBound);
            sb.AppendTabFormatLine(1, "float lonExtent = {0} - {1};", this.EastLonBound, this.WestLonBound);
            sb.AppendTabFormatLine(1, "float curLat = {0} + (y + 1.0f) * latExtent / 2.0f;", this.SouthLatBound);
            sb.AppendTabFormatLine(1, "float curLon = {0} + (x + 1.0f) * lonExtent / 2.0f;", this.WestLonBound);
            sb.AppendTabFormatLine(1, "float3 coords = Model_Sphere(curLat, curLon) + {0};", this.SeedFloat);
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
                this.SouthLatBound,
                this.NorthLatBound,
                this.WestLonBound,
                this.EastLonBound);

            return sb.ToString();
        }
    }
}
