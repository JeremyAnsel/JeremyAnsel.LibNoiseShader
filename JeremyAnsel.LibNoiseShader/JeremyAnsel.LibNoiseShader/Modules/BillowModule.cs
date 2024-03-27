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
            string key = nameof(BillowModule);

            header.AppendTabFormatLine("float {0}_Frequency = 1.0f;", key);
            header.AppendTabFormatLine("float {0}_Lacunarity = 2.0f;", key);
            header.AppendTabFormatLine("int {0}_OctaveCount = 6;", key);
            header.AppendTabFormatLine("float {0}_Persistence = 0.5f;", key);
            header.AppendTabFormatLine("int {0}_SeedOffset = 0;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(BillowModule);

            body.AppendTabFormatLine(2, "{0}_Frequency = {1};", key, this.Frequency);
            body.AppendTabFormatLine(2, "{0}_Lacunarity = {1};", key, this.Lacunarity);
            body.AppendTabFormatLine(2, "{0}_OctaveCount = {1};", key, this.octaveCount);
            body.AppendTabFormatLine(2, "{0}_Persistence = {1};", key, this.Persistence);
            body.AppendTabFormatLine(2, "{0}_SeedOffset = {1};", key, this.SeedOffset);
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
            string key = nameof(BillowModule);

            body.AppendTabFormatLine(2, "float3 freq = p * {0}_Frequency;", key);
            body.AppendTabFormatLine(2, "float curPersistence = 1.0f;");
            body.AppendTabFormatLine(2, "float value = 0.0f;");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "[fastopt] for (int curOctave = 0; curOctave < {0}_OctaveCount; curOctave++)", key);
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "int seed = ({0}_SeedOffset + curOctave) & 0x7fffffff;", key);
            body.AppendTabFormatLine(3, "float signal = Noise3D_GradientCoherent(freq + seed);");
            body.AppendTabFormatLine(3, "signal = 2.0f * signal * sign(signal) - 1.0f;");
            body.AppendTabFormatLine(3, "value += signal * curPersistence;");
            body.AppendTabFormatLine(3, "freq *= {0}_Lacunarity;", key);
            body.AppendTabFormatLine(3, "curPersistence *= {0}_Persistence;", key);
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "value += 0.5f;");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "result = value;");
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
