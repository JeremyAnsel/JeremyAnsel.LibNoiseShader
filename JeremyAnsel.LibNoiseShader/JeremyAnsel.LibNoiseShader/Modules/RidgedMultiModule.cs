using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class RidgedMultiModule : ModuleBase
    {
        public const int MinOctave = 1;

        public const int MaxOctave = 30;

        private readonly Noise3D noise;

        private readonly float[] spectralWeights = new float[RidgedMultiModule.MaxOctave];

        private int octaveCount;

        private float lacunarity;

        private float exponent;

        public RidgedMultiModule(Noise3D noise)
        {
            this.noise = noise ?? throw new ArgumentNullException(nameof(noise));

            this.Frequency = 1.0f;
            this.Lacunarity = 2.0f;
            this.Offset = 1.0f;
            this.Gain = 2.0f;
            this.Exponent = 1.0f;
            this.OctaveCount = 6;
            this.SeedOffset = 0;
        }

        public float Frequency { get; set; }

        public float Lacunarity
        {
            get
            {
                return this.lacunarity;
            }

            set
            {
                this.lacunarity = value;
                this.CalcSpectralWeights();
            }
        }

        public float Offset { get; set; }

        public float Gain { get; set; }

        public float Exponent
        {
            get
            {
                return this.exponent;
            }

            set
            {
                this.exponent = value;
                this.CalcSpectralWeights();
            }
        }

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

        public int SeedOffset { get; set; }

        public override int RequiredSourceModuleCount => 0;

        public override float GetValue(float x, float y, float z)
        {
            x *= this.Frequency;
            y *= this.Frequency;
            z *= this.Frequency;

            float value = 0.0f;
            float weight = 1.0f;
            float offset = this.Offset;
            float gain = this.Gain;

            for (int curOctave = 0; curOctave < this.OctaveCount; curOctave++)
            {
                // Get the coherent-noise value.
                int seed = (this.SeedOffset + curOctave) & 0x7fffffff;
                float signal = this.noise.GradientCoherent(x + seed, y + seed, z + seed);

                // Make the ridges.
                signal = Math.Abs(signal);
                signal = offset - signal;

                // Square the signal to increase the sharpness of the ridges.
                signal *= signal;

                // The weighting from the previous octave is applied to the signal.
                // Larger values have higher weights, producing sharp points along the
                // ridges.
                signal *= weight;

                // Weight successive contributions by the previous signal.
                weight = signal * gain;
                if (weight > 1.0f)
                {
                    weight = 1.0f;
                }
                else if (weight < 0.0f)
                {
                    weight = 0.0f;
                }

                // Add the signal to the output value.
                value += signal * this.spectralWeights[curOctave];

                // Go to the next octave.
                x *= this.Lacunarity;
                y *= this.Lacunarity;
                z *= this.Lacunarity;
            }

            return (value * 1.25f) - 1.0f;
        }

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "static const float spectralWeights[{0}] =", this.spectralWeights.Length);
            sb.AppendTabFormatLine(1, "{");

            for (int i = 0; i < this.spectralWeights.Length; i++)
            {
                sb.AppendTabFormatLine(2, "{0},", this.spectralWeights[i]);
            }

            sb.AppendTabFormatLine(1, "};");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float3 freq = float3(x, y, z) * {0};", this.Frequency);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float value = 0.0f;");
            sb.AppendTabFormatLine(1, "float weight = 1.0f;");
            sb.AppendTabFormatLine(1, "float offset = {0};", this.Offset);
            sb.AppendTabFormatLine(1, "float gain = {0};", this.Gain);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "[fastopt] for (int curOctave = 0; curOctave < {0}; curOctave++)", this.OctaveCount);
            sb.AppendTabFormatLine(1, "{");
            sb.AppendTabFormatLine(2, "int seed = ({0} + curOctave) & 0x7fffffff;", this.SeedOffset);
            sb.AppendTabFormatLine(2, "float signal = Noise3D_GradientCoherent(freq + seed);");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(2, "signal = signal * sign(signal);");
            sb.AppendTabFormatLine(2, "signal = offset - signal;");
            sb.AppendTabFormatLine(2, "signal *= signal;");
            sb.AppendTabFormatLine(2, "signal *= weight;");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(2, "weight = signal * gain;");
            sb.AppendTabFormatLine(2, "weight = clamp(weight, 0.0f, 1.0f);");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(2, "value += signal * spectralWeights[curOctave];");
            sb.AppendTabFormatLine(2, "freq *= {0};", this.Lacunarity);
            sb.AppendTabFormatLine(1, "}");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "return value * 1.25f - 1.0f;");
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
            sb.AppendTabFormatLine(1, "Offset = {0},", this.Offset);
            sb.AppendTabFormatLine(1, "Gain = {0},", this.Gain);
            sb.AppendTabFormatLine(1, "Exponent = {0},", this.Exponent);
            sb.AppendTabFormatLine(1, "OctaveCount = {0},", this.OctaveCount);
            sb.AppendTabFormatLine(1, "SeedOffset = {0},", this.SeedOffset);
            sb.AppendTabFormatLine("};");

            return sb.ToString();
        }

        private void CalcSpectralWeights()
        {
            float h = this.Exponent;
            float frequency = 1.0f;

            for (int i = 0; i < this.spectralWeights.Length; i++)
            {
                this.spectralWeights[i] = (float)Math.Pow(frequency, -h);
                frequency *= this.Lacunarity;
            }
        }
    }
}
