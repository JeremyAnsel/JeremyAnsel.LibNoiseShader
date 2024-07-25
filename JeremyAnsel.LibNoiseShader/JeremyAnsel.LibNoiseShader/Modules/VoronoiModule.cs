using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class VoronoiModule : ModuleBase
    {
        private readonly Noise3D noise;

        public VoronoiModule(Noise3D? noise)
        {
            this.noise = noise ?? throw new ArgumentNullException(nameof(noise));

            this.IsDistanceApplied = false;
            this.Displacement = 1.0f;
            this.Frequency = 1.0f;
            this.SeedOffset = 0;
        }

        public bool IsDistanceApplied { get; set; }

        public float Displacement { get; set; }

        public float Frequency { get; set; }

        public int SeedOffset { get; set; }

        public override int RequiredSourceModuleCount => 0;

        public override float GetValue(float x, float y, float z)
        {
            int seed = this.SeedOffset;

            x *= this.Frequency;
            y *= this.Frequency;
            z *= this.Frequency;

            int xInt = (int)Math.Floor(x);
            int yInt = (int)Math.Floor(y);
            int zInt = (int)Math.Floor(z);

            float minDist = 46340.0f * 46340.0f;
            float xCandidate = 0;
            float yCandidate = 0;
            float zCandidate = 0;

            // Inside each unit cube, there is a seed point at a random position.  Go
            // through each of the nearby cubes until we find a cube with a seed point
            // that is closest to the specified position.
            for (int zCur = zInt - 2; zCur <= zInt + 2; zCur++)
            {
                for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++)
                {
                    for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++)
                    {
                        // Calculate the position and distance to the seed point inside of
                        // this unit cube.
                        float xPos = xCur + this.noise.IntValue(xCur + seed + 1, yCur + seed + 1, zCur + seed + 1);
                        float yPos = yCur + this.noise.IntValue(xCur + seed + 2, yCur + seed + 2, zCur + seed + 2);
                        float zPos = zCur + this.noise.IntValue(xCur + seed + 3, yCur + seed + 3, zCur + seed + 3);
                        float xDist = xPos - x;
                        float yDist = yPos - y;
                        float zDist = zPos - z;
                        float dist = xDist * xDist + yDist * yDist + zDist * zDist;

                        if (dist < minDist)
                        {
                            // This seed point is closer to any others found so far, so record
                            // this seed point.
                            minDist = dist;
                            xCandidate = xPos;
                            yCandidate = yPos;
                            zCandidate = zPos;
                        }
                    }
                }
            }

            float value;

            if (this.IsDistanceApplied)
            {
                // Determine the distance to the nearest seed point.
                value = (float)Math.Sqrt(minDist) * (float)Math.Sqrt(3.0f) - 1.0f;
            }
            else
            {
                value = 0.0f;
            }

            // Return the calculated distance with the displacement value applied.
            int xCandidateInt = (int)Math.Floor(xCandidate);
            int yCandidateInt = (int)Math.Floor(yCandidate);
            int zCandidateInt = (int)Math.Floor(zCandidate);
            return value + (this.Displacement * this.noise.IntValue(xCandidateInt + seed, yCandidateInt + seed, zCandidateInt + seed));
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
            string key = nameof(VoronoiModule);

            header.AppendTabFormatLine("bool {0}_IsDistanceApplied = false;", key);
            header.AppendTabFormatLine("float {0}_Displacement = 1.0f;", key);
            header.AppendTabFormatLine("float {0}_Frequency = 1.0f;", key);
            header.AppendTabFormatLine("int {0}_SeedOffset = 0;", key);
        }

        public override bool HasHlslSettings()
        {
            return true;
        }

        public override void EmitHlslSettings(StringBuilder body)
        {
            string key = nameof(VoronoiModule);

            body.AppendTabFormatLine(2, "{0}_IsDistanceApplied = {1};", key, this.IsDistanceApplied ? "true" : "false");
            body.AppendTabFormatLine(2, "{0}_Displacement = {1};", key, this.Displacement);
            body.AppendTabFormatLine(2, "{0}_Frequency = {1};", key, this.Frequency);
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
            string key = nameof(VoronoiModule);

            body.AppendTabFormatLine(2, "float3 freq = p * {0}_Frequency;", key);
            body.AppendTabFormatLine(2, "int3 xyz = (int3)floor(freq);");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "float minDist = {0};", 46340.0f * 46340.0f);
            body.AppendTabFormatLine(2, "float3 candidate = 0;");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "[fastopt] for (int zCur = xyz.z - 2; zCur <= xyz.z + 2; zCur++)");
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "[fastopt] for (int yCur = xyz.y - 2; yCur <= xyz.y + 2; yCur++)");
            body.AppendTabFormatLine(3, "{");
            body.AppendTabFormatLine(4, "[fastopt] for (int xCur = xyz.x - 2; xCur <= xyz.x + 2; xCur++)");
            body.AppendTabFormatLine(4, "{");
            body.AppendTabFormatLine(5, "int3 cur = int3(xCur, yCur, zCur);");
            body.AppendTabFormatLine(5, "float3 pos;");
            body.AppendTabFormatLine(5, "pos.x = xCur + Noise3D_IntValue(cur + {0}_SeedOffset + 1);", key);
            body.AppendTabFormatLine(5, "pos.y = yCur + Noise3D_IntValue(cur + {0}_SeedOffset + 2);", key);
            body.AppendTabFormatLine(5, "pos.z = zCur + Noise3D_IntValue(cur + {0}_SeedOffset + 3);", key);
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(5, "float3 d = pos - freq;");
            body.AppendTabFormatLine(5, "float dist = dot(d, d);");
            body.AppendTabFormatLine();
            //body.AppendTabFormatLine(5, "[branch] if (dist < minDist)");
            body.AppendTabFormatLine(5, "if (dist < minDist)");
            body.AppendTabFormatLine(5, "{");
            body.AppendTabFormatLine(6, "minDist = dist;");
            body.AppendTabFormatLine(6, "candidate = pos;");
            body.AppendTabFormatLine(5, "}");
            body.AppendTabFormatLine(4, "}");
            body.AppendTabFormatLine(3, "}");
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine();
            body.AppendTabFormatLine(2, "float value;");
            body.AppendTabFormatLine(2, "if ({0}_IsDistanceApplied)", key);
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "value = sqrt(minDist) * sqrt(3.0f) - 1.0f;");
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine(2, "else");
            body.AppendTabFormatLine(2, "{");
            body.AppendTabFormatLine(3, "value = 0.0f;");
            body.AppendTabFormatLine(2, "}");
            body.AppendTabFormatLine(2, "int3 candidateInt = (int3)floor(candidate);");
            body.AppendTabFormatLine(2, "result = value + ({0}_Displacement * Noise3D_IntValue(candidateInt + {0}_SeedOffset));", key);
        }

        public override string GetCSharpBody(CSharpContext context)
        {
            var sb = new StringBuilder();
            string name = context.GetModuleName(this);
            string type = context.GetModuleType(this);

            sb.AppendTabFormatLine("{0} {1} = new(noise)", type, name);
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "IsDistanceApplied = {0},", this.IsDistanceApplied);
            sb.AppendTabFormatLine(1, "Displacement = {0},", this.Displacement);
            sb.AppendTabFormatLine(1, "Frequency = {0},", this.Frequency);
            sb.AppendTabFormatLine(1, "SeedOffset = {0},", this.SeedOffset);
            sb.AppendTabFormatLine("};");

            return sb.ToString();
        }
    }
}
