using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class BillowModule : ModuleBase
    {
        public const int MinOctave = 1;

        public const int MaxOctave = 30;

        private readonly Noise3D noise;

        private int octaveCount;

        public BillowModule(Noise3D noise)
        {
            this.noise = noise ?? throw new ArgumentNullException(nameof(noise));

            this.Frequency = 1.0f;
            this.Lacunarity = 2.0f;
            this.OctaveCount = 6;
            this.Persistence = 0.5f;
            this.SeedOffset = 0;
        }

        public float Frequency { get; set; }

        public float Lacunarity { get; set; }

        public int OctaveCount
        {
            get
            {
                return this.octaveCount;
            }

            set
            {
                value = Math.Min(Math.Max(value, MinOctave), MaxOctave);
                this.octaveCount = value;
            }
        }

        public float Persistence { get; set; }

        public int SeedOffset { get; set; }

        public override int RequiredSourceModuleCount => 0;

        public override float GetValue(float x, float y, float z)
        {
            x *= this.Frequency;
            y *= this.Frequency;
            z *= this.Frequency;

            float curPersistence = 1.0f;
            float value = 0.0f;

            for (int curOctave = 0; curOctave < this.OctaveCount; curOctave++)
            {
                int seed = (this.SeedOffset + curOctave) & 0x7fffffff;
                float signal = this.noise.GradientCoherent(x + seed, y + seed, z + seed);
                signal = 2.0f * Math.Abs(signal) - 1.0f;
                value += signal * curPersistence;

                x *= this.Lacunarity;
                y *= this.Lacunarity;
                z *= this.Lacunarity;
                curPersistence *= this.Persistence;
            }

            value += 0.5f;

            return value;
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float3 freq = float3(x, y, z) * {0};", this.Frequency);
            sb.AppendTabFormatLine(1, "float curPersistence = 1.0f;");
            sb.AppendTabFormatLine(1, "float value = 0.0f;");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "[fastopt] for (int curOctave = 0; curOctave < {0}; curOctave++)", this.OctaveCount);
            sb.AppendTabFormatLine(1, "{");
            sb.AppendTabFormatLine(2, "int seed = ({0} + curOctave) & 0x7fffffff;", this.SeedOffset);
            sb.AppendTabFormatLine(2, "float signal = Noise3D_GradientCoherent(freq + seed);");
            sb.AppendTabFormatLine(2, "signal = 2.0f * signal * sign(signal) - 1.0f;");
            sb.AppendTabFormatLine(2, "value += signal * curPersistence;");
            sb.AppendTabFormatLine(2, "freq *= {0};", this.Lacunarity);
            sb.AppendTabFormatLine(2, "curPersistence *= {0};", this.Persistence);
            sb.AppendTabFormatLine(1, "}");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "value += 0.5f;");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "return value;");
            sb.AppendTabFormatLine("}");

            return sb.ToString();
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new(noise)", type, name);
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "Frequency = {0},", this.Frequency);
            sb.AppendTabFormatLine(1, "Lacunarity = {0},", this.Lacunarity);
            sb.AppendTabFormatLine(1, "OctaveCount = {0},", this.OctaveCount);
            sb.AppendTabFormatLine(1, "Persistence = {0},", this.Persistence);
            sb.AppendTabFormatLine(1, "SeedOffset = {0},", this.SeedOffset);
            sb.AppendTabFormatLine("};");

            return sb.ToString();
        }
    }
}
