using System;
using System.Text;

namespace JeremyAnsel.LibNoiseShader.Modules
{
    public sealed class VoronoiModule : ModuleBase
    {
        private readonly Noise3D noise;

        public VoronoiModule(Noise3D noise)
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

            float minDist = float.MaxValue;
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

        public override string GetHlslBody(HlslContext context)
        {
            var sb = new StringBuilder();

            sb.AppendTabFormatLine(context.GetModuleFunctionDefinition(this));
            sb.AppendTabFormatLine("{");
            sb.AppendTabFormatLine(1, "float3 freq = float3(x, y, z) * {0};", this.Frequency);
            sb.AppendTabFormatLine(1, "int xInt = (int)floor(freq.x);");
            sb.AppendTabFormatLine(1, "int yInt = (int)floor(freq.y);");
            sb.AppendTabFormatLine(1, "int zInt = (int)floor(freq.z);");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "float minDist = {0};", float.MaxValue);
            sb.AppendTabFormatLine(1, "float xCandidate = 0;");
            sb.AppendTabFormatLine(1, "float yCandidate = 0;");
            sb.AppendTabFormatLine(1, "float zCandidate = 0;");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(1, "[fastopt] for (int zCur = zInt - 2; zCur <= zInt + 2; zCur++)");
            sb.AppendTabFormatLine(1, "{");
            sb.AppendTabFormatLine(2, "[fastopt] for (int yCur = yInt - 2; yCur <= yInt + 2; yCur++)");
            sb.AppendTabFormatLine(2, "{");
            sb.AppendTabFormatLine(3, "[fastopt] for (int xCur = xInt - 2; xCur <= xInt + 2; xCur++)");
            sb.AppendTabFormatLine(3, "{");
            sb.AppendTabFormatLine(4, "float xPos = xCur + Noise3D_IntValue(int3(xCur, yCur, zCur) + {0} + 1);", this.SeedOffset);
            sb.AppendTabFormatLine(4, "float yPos = yCur + Noise3D_IntValue(int3(xCur, yCur, zCur) + {0} + 2);", this.SeedOffset);
            sb.AppendTabFormatLine(4, "float zPos = zCur + Noise3D_IntValue(int3(xCur, yCur, zCur) + {0} + 3);", this.SeedOffset);
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(4, "float xDist = xPos - freq.x;");
            sb.AppendTabFormatLine(4, "float yDist = yPos - freq.y;");
            sb.AppendTabFormatLine(4, "float zDist = zPos - freq.z;");
            sb.AppendTabFormatLine(4, "float dist = xDist * xDist + yDist * yDist + zDist * zDist;");
            sb.AppendTabFormatLine();
            sb.AppendTabFormatLine(4, "[branch] if (dist < minDist)");
            sb.AppendTabFormatLine(4, "{");
            sb.AppendTabFormatLine(5, "minDist = dist;");
            sb.AppendTabFormatLine(5, "xCandidate = xPos;");
            sb.AppendTabFormatLine(5, "yCandidate = yPos;");
            sb.AppendTabFormatLine(5, "zCandidate = zPos;");
            sb.AppendTabFormatLine(4, "}");
            sb.AppendTabFormatLine(3, "}");
            sb.AppendTabFormatLine(2, "}");
            sb.AppendTabFormatLine(1, "}");
            sb.AppendTabFormatLine();

            if (this.IsDistanceApplied)
            {
                sb.AppendTabFormatLine(1, "float value = sqrt(minDist) * sqrt(3.0f) - 1.0f;");
                sb.AppendTabFormatLine();
            }
            else
            {
                sb.AppendTabFormatLine(1, "float value = 0.0f;");
                sb.AppendTabFormatLine();
            }

            sb.AppendTabFormatLine(1, "int3 candidateInt = (int3)floor(float3(xCandidate, yCandidate, zCandidate));");
            sb.AppendTabFormatLine(1, "return value + ({0} * Noise3D_IntValue(candidateInt + {1}));", this.Displacement, this.SeedOffset);
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
            sb.AppendTabFormatLine(1, "IsDistanceApplied = {0},", this.IsDistanceApplied);
            sb.AppendTabFormatLine(1, "Displacement = {0},", this.Displacement);
            sb.AppendTabFormatLine(1, "Frequency = {0},", this.Frequency);
            sb.AppendTabFormatLine(1, "SeedOffset = {0},", this.SeedOffset);
            sb.AppendTabFormatLine("};");

            return sb.ToString();
        }
    }
}
