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

        public TurbulenceModule(Noise3D? noise, IModule? module0)
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
            return this.GetSourceModule(0)!.GetValue(distortX, distortY, distortZ);
        }

        public override int EmitHlslMaxDepth()
        {
            return 4;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this.distortXModule);
            context.EmitHeader(this.distortYModule);
            context.EmitHeader(this.distortZModule);
            context.EmitHeader(this);
            context.EmitCoords(this, 0, false);
            context.EmitSettings(this.distortXModule);
            context.EmitFunction(this.distortXModule, true);
            context.EmitCoords(this, 1, false);
            context.EmitSettings(this.distortYModule);
            context.EmitFunction(this.distortYModule, true);
            context.EmitCoords(this, 2, false);
            context.EmitSettings(this.distortZModule);
            context.EmitFunction(this.distortZModule, true);
            context.EmitCoords(this, 3, false);
            this.GetSourceModule(0)!.EmitHlsl(context);
            context.EmitSettings(this);
            context.EmitFunction(this, true);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            string key = nameof(TurbulenceModule);

            header.AppendTabFormatLine("float {0}_Frequency = 1.0f;", key);
            header.AppendTabFormatLine("float {0}_Power = 1.0f;", key);
            header.AppendTabFormatLine("int {0}_Roughness = 3;", key);
            header.AppendTabFormatLine("int {0}_SeedOffset = 0;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(TurbulenceModule);

            body.AppendTabFormatLine(2, "{0}_Frequency = {1};", key, this.Frequency);
            body.AppendTabFormatLine(2, "{0}_Power = {1};", key, this.Power);
            body.AppendTabFormatLine(2, "{0}_Roughness = {1};", key, this.Roughness);
            body.AppendTabFormatLine(2, "{0}_SeedOffset = {1};", key, this.SeedOffset);
        }

        public override bool HasHlslCoords(int index)
        {
            return true;
        }

        public override void EmitHlslCoords(StringBuilder body, int index)
        {
            switch (index)
            {
                case 0:
                    body.AppendTabFormatLine(2, "float x0 = coords.x + (12414.0f / 65536.0f);");
                    body.AppendTabFormatLine(2, "float y0 = coords.y + (65124.0f / 65536.0f);");
                    body.AppendTabFormatLine(2, "float z0 = coords.z + (31337.0f / 65536.0f);");
                    body.AppendTabFormatLine(2, "coords = float3(x0, y0, z0);");
                    break;

                case 1:
                    body.AppendTabFormatLine(2, "float x1 = coords.x + (26519.0f / 65536.0f);");
                    body.AppendTabFormatLine(2, "float y1 = coords.y + (18128.0f / 65536.0f);");
                    body.AppendTabFormatLine(2, "float z1 = coords.z + (60493.0f / 65536.0f);");
                    body.AppendTabFormatLine(2, "coords = float3(x1, y1, z1);");
                    break;

                case 2:
                    body.AppendTabFormatLine(2, "float x2 = coords.x + (53820.0f / 65536.0f);");
                    body.AppendTabFormatLine(2, "float y2 = coords.y + (11213.0f / 65536.0f);");
                    body.AppendTabFormatLine(2, "float z2 = coords.z + (44845.0f / 65536.0f);");
                    body.AppendTabFormatLine(2, "coords = float3(x2, y2, z2);");
                    break;

                case 3:
                    body.AppendTabFormatLine(2, "modules_results_index -= 3;");
                    body.AppendTabFormatLine(2, "float param0 = modules_results[modules_results_index];");
                    body.AppendTabFormatLine(2, "float param1 = modules_results[modules_results_index + 1];");
                    body.AppendTabFormatLine(2, "float param2 = modules_results[modules_results_index + 2];");
                    body.AppendTabFormatLine(2, "float distortX = coords.x + param0 * {0};", this.Power);
                    body.AppendTabFormatLine(2, "float distortY = coords.y + param1 * {0};", this.Power);
                    body.AppendTabFormatLine(2, "float distortZ = coords.z + param2 * {0};", this.Power);
                    body.AppendTabFormatLine(2, "coords = float3(distortX, distortY, distortZ);");
                    break;
            }
        }

        public override int GetHlslFunctionParametersCount()
        {
            return 1;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            body.AppendTabFormatLine(2, "return param0;");
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
