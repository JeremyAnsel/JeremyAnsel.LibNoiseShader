using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class TurbulenceModule : ModuleBase
    {
        private float frequency;

        private int roughness;

        private int seedOffset;

        private readonly PerlinModule distortXModule;

        private readonly PerlinModule distortYModule;

        private readonly PerlinModule distortZModule;

        public TurbulenceModule(Noise3D noise, IModule module0)
        {
            if (noise is null)
            {
                throw new ArgumentNullException(nameof(noise));
            }

            this.SetSourceModule(0, module0);

            this.distortXModule = new PerlinModule(noise);
            this.distortYModule = new PerlinModule(noise);
            this.distortZModule = new PerlinModule(noise);

            this.Frequency = 1.0f;
            this.Power = 1.0f;
            this.Roughness = 3;
            this.SeedOffset = 0;
        }

        public float Frequency
        {
            get
            {
                return this.frequency;
            }

            set
            {
                this.frequency = value;
                this.distortXModule.Frequency = value;
                this.distortYModule.Frequency = value;
                this.distortZModule.Frequency = value;
            }
        }

        public float Power { get; set; }

        public int Roughness
        {
            get
            {
                return this.roughness;
            }

            set
            {
                this.roughness = value;
                this.distortXModule.OctaveCount = value;
                this.distortYModule.OctaveCount = value;
                this.distortZModule.OctaveCount = value;
            }
        }

        public int SeedOffset
        {
            get
            {
                return this.seedOffset;
            }

            set
            {
                this.seedOffset = value;
                this.distortXModule.SeedOffset = value;
                this.distortYModule.SeedOffset = value + 1;
                this.distortZModule.SeedOffset = value + 2;
            }
        }

        public override int RequiredSourceModuleCount => 1;

        public override float GetValue(float x, float y, float z)
        {
            // Get the values from the three Perlin noise modules and
            // add each value to each coordinate of the input value.  There are also
            // some offsets added to the coordinates of the input values.  This prevents
            // the distortion modules from returning zero if the (x, y, z) coordinates,
            // when multiplied by the frequency, are near an integer boundary.  This is
            // due to a property of gradient coherent noise, which returns zero at
            // integer boundaries.

            float x0 = x + (12414.0f / 65536.0f);
            float y0 = y + (65124.0f / 65536.0f);
            float z0 = z + (31337.0f / 65536.0f);
            float x1 = x + (26519.0f / 65536.0f);
            float y1 = y + (18128.0f / 65536.0f);
            float z1 = z + (60493.0f / 65536.0f);
            float x2 = x + (53820.0f / 65536.0f);
            float y2 = y + (11213.0f / 65536.0f);
            float z2 = z + (44845.0f / 65536.0f);

            float distortX = x + (this.distortXModule.GetValue(x0, y0, z0) * this.Power);
            float distortY = y + (this.distortYModule.GetValue(x1, y1, z1) * this.Power);
            float distortZ = z + (this.distortZModule.GetValue(x2, y2, z2) * this.Power);

            // Retrieve the output value at the offsetted input value instead of the
            // original input value.
            return this.GetSourceModule(0).GetValue(distortX, distortY, distortZ);
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string distortXModule = context.GetModuleName(this.distortXModule);
            string distortYModule = context.GetModuleName(this.distortYModule);
            string distortZModule = context.GetModuleName(this.distortZModule);

            sb.AppendTabFormatLine(this.distortXModule.GetHlslBody(context));
            sb.AppendTabFormatLine(this.distortYModule.GetHlslBody(context));
            sb.AppendTabFormatLine(this.distortZModule.GetHlslBody(context));

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float x0 = x + (12414.0f / 65536.0f);");
            sb.AppendTabFormatLine(1, "float y0 = y + (65124.0f / 65536.0f);");
            sb.AppendTabFormatLine(1, "float z0 = z + (31337.0f / 65536.0f);");
            sb.AppendTabFormatLine(1, "float x1 = x + (26519.0f / 65536.0f);");
            sb.AppendTabFormatLine(1, "float y1 = y + (18128.0f / 65536.0f);");
            sb.AppendTabFormatLine(1, "float z1 = z + (60493.0f / 65536.0f);");
            sb.AppendTabFormatLine(1, "float x2 = x + (53820.0f / 65536.0f);");
            sb.AppendTabFormatLine(1, "float y2 = y + (11213.0f / 65536.0f);");
            sb.AppendTabFormatLine(1, "float z2 = z + (44845.0f / 65536.0f);");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float distortX = x + {0}(x0, y0, z0) * {1};", distortXModule, this.Power);
            sb.AppendTabFormatLine(1, "float distortY = y + {0}(x1, y1, z1) * {1};", distortYModule, this.Power);
            sb.AppendTabFormatLine(1, "float distortZ = z + {0}(x2, y2, z2) * {1};", distortZModule, this.Power);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "return {0}(distortX, distortY, distortZ);", module0);
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string module0 = context.GetModuleName(this.GetSourceModule(0));
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new(noise, {2})", type, name, module0);
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "Frequency = {0},", this.Frequency);
            sb.AppendTabFormatLine(1, "Power = {0},", this.Power);
            sb.AppendTabFormatLine(1, "Roughness = {0},", this.Roughness);
            sb.AppendTabFormatLine(1, "SeedOffset = {0},", this.SeedOffset);
            sb.AppendTabFormatLine("};");

            return sb.ToString();
        }
    }
}
