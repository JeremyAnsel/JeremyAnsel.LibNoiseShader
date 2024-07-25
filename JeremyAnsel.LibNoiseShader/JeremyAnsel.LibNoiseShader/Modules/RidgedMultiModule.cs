using System;
using System.Linq;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class RidgedMultiModule : ModuleBase
    {
        public const int MinOctave = 1;

        public const int MaxOctave = 30;

        private readonly Noise3D noise;

        private readonly float[] spectralWeights = new float[MaxOctave];

        private int octaveCount;

        private float lacunarity;

        private float exponent;

        public RidgedMultiModule(Noise3D? noise)
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

        public override int EmitHlslMaxDepth()
        {
            return 0;
        }

        public override void EmitHlsl(HlslContext context)
        {
            context.EmitHeader(this);
            context.EmitSettings(this);
            context.EmitFunction(this, false);
        }

        public override void EmitHlslHeader(HlslContext context, StringBuilder header)
        {
            string key = nameof(RidgedMultiModule);

            header.AppendTabFormatLine("float {0}_Frequency = 1.0f;", key);
            header.AppendTabFormatLine("float {0}_Lacunarity = 2.0f;", key);
            header.AppendTabFormatLine("float {0}_Offset = 1.0f;", key);
            header.AppendTabFormatLine("float {0}_Gain = 2.0f;", key);
            header.AppendTabFormatLine("float {0}_Exponent = 1.0f;", key);
            header.AppendTabFormatLine("int {0}_OctaveCount = 6;", key);
            header.AppendTabFormatLine("int {0}_SeedOffset = 0;", key);

            header.AppendTabFormatLine(
                "float {0}_SpectralWeights[{1}] = {{ {2} }};",
                key,
                MaxOctave,
                string.Concat(Enumerable.Repeat("0.0f,", MaxOctave)));
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(RidgedMultiModule);

            body.AppendTabFormatLine(2, "{0}_Frequency = {1};", key, this.Frequency);
            body.AppendTabFormatLine(2, "{0}_Lacunarity = {1};", key, this.Lacunarity);
            body.AppendTabFormatLine(2, "{0}_Offset = {1};", key, this.Offset);
            body.AppendTabFormatLine(2, "{0}_Gain = {1};", key, this.Gain);
            body.AppendTabFormatLine(2, "{0}_Exponent = {1};", key, this.Exponent);
            body.AppendTabFormatLine(2, "{0}_OctaveCount = {1};", key, this.OctaveCount);
            body.AppendTabFormatLine(2, "{0}_SeedOffset = {1};", key, this.SeedOffset);

            for (int index = 0; index < this.OctaveCount; index++)
            {
                body.AppendTabFormatLine(2, "{0}_SpectralWeights[{1}] = {2};", key, index, this.spectralWeights[index]);
            }
        }

        public override bool HasHlslCoords(int index)
        {
            return false;
        }

        public override void EmitHlslCoords(StringBuilder body, int index)
        {
        }

        public override int GetHlslFunctionParametersCount()
        {
            return 0;
        }

        public override void EmitHlslFunction(StringBuilder body)
        {
            string key = nameof(RidgedMultiModule);

            body.AppendTabFormatLine(2, "float3 freq = p * {0}_Frequency;", key);
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "float value = 0.0f;");
            body.AppendTabFormatLine(2, "float weight = 1.0f;");
            body.AppendTabFormatLine(2, "float offset = {0}_Offset;", key);
            body.AppendTabFormatLine(2, "float gain = {0}_Gain;", key);
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "[fastopt] for (int curOctave = 0; curOctave < {0}_OctaveCount; curOctave++)", key);
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "int seed = ({0}_SeedOffset + curOctave) & 0x7fffffff;", key);
            body.AppendTabFormatLine(3, "float signal = Noise3D_GradientCoherent(freq + seed);");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(3, "signal = signal * sign(signal);");
            body.AppendTabFormatLine(3, "signal = offset - signal;");
            body.AppendTabFormatLine(3, "signal *= signal;");
            body.AppendTabFormatLine(3, "signal *= weight;");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(3, "weight = signal * gain;");
            body.AppendTabFormatLine(3, "weight = clamp(weight, 0.0f, 1.0f);");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(3, "value += signal * {0}_SpectralWeights[curOctave];", key);
            body.AppendTabFormatLine(3, "freq *= {0}_Lacunarity;", key);
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "result = value * 1.25f - 1.0f;");
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
