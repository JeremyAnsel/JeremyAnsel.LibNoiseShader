using System;
using System.Collections.Generic;

namespace JeremyAnsel.LibNoiseShader
{
    public sealed class Noise3D
    {
        private static readonly Float3[] _g =
        {
            new Float3(1, 1, 0),
            new Float3(-1, 1, 0),
            new Float3(1, -1, 0),
            new Float3(-1, -1, 0),
            new Float3(1, 0, 1),
            new Float3(-1, 0, 1),
            new Float3(1, 0, -1),
            new Float3(-1, 0, -1),
            new Float3(0, 1, 1),
            new Float3(0, -1, 1),
            new Float3(0, 1, -1),
            new Float3(0, -1, -1)
        };

        private readonly int[] _permutation;

        private readonly int[] _permutation2D;

        private readonly float[] _permutationGrad;

        public Noise3D(int seed)
        {
            this.Seed = seed;
            _permutation = GeneratePermutationTable(seed);
            _permutation2D = GeneratePerm2DBuffer();
            _permutationGrad = GenerateGradPermTexture();
        }

        public int Seed { get; }

        private static int[] GeneratePermutationTable(int seed)
        {
            var values = new List<int>(256);
            for (int i = 0; i < 256; i++)
            {
                values.Add(i);
            }

            var random = new Random(seed);

            var perm = new int[256];
            for (int i = 0; i < 256; i++)
            {
                int index = random.Next(values.Count);
                perm[i] = values[index];
                values.RemoveAt(index);
            }

            return perm;
        }

        public static string HeaderHlsl()
        {
            return @"
Texture1D<uint> permBuffer : register(t0);
Texture2D<uint4> perm2dBuffer : register(t1);
Texture1D<float4> gradPermBuffer : register(t2);
".NormalizeEndLines();
        }

        public int[] RetrievePermutationBuffer()
        {
            return _permutation;
        }

        private int[] GeneratePerm2DBuffer()
        {
            int[] perm2D = new int[256 * 256 * 4];

            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    Perm2D(x, y, out int AA, out int AB, out int BA, out int BB);

                    perm2D[(y * 256 + x) * 4 + 0] = AA;
                    perm2D[(y * 256 + x) * 4 + 1] = AB;
                    perm2D[(y * 256 + x) * 4 + 2] = BA;
                    perm2D[(y * 256 + x) * 4 + 3] = BB;
                }
            }

            return perm2D;
        }

        public int[] RetrievePerm2DBuffer()
        {
            return _permutation2D;
        }

        private float[] GenerateGradPermTexture()
        {
            float[] grad = new float[512 * 4];

            for (int i = 0; i < 512; i++)
            {
                int index = Perm(i) % 12;
                Float3 g = _g[index];

                grad[i * 4 + 0] = g.X;
                grad[i * 4 + 1] = g.Y;
                grad[i * 4 + 2] = g.Z;
                grad[i * 4 + 3] = 0;
            }

            return grad;
        }

        public float[] RetrieveGradPermTexture()
        {
            return _permutationGrad;
        }

        public int Perm(int x)
        {
            return _permutation[(uint)x % 256];
        }

        public static string PermHlsl()
        {
            return @"
int Noise3D_Perm(int x)
{
    return permBuffer.Load(int2((uint)x % 256, 0));
}
".NormalizeEndLines();
        }


        public float IntValue(int piX, int piY, int piZ)
        {
            int A = Perm(piX) + piY;
            int B = Perm(A) + piZ;
            int V = Perm(B);
            return V / 255.0f * 2.0f - 1.0f;
        }

        public static string IntValueHlsl()
        {
            return @"
float Noise3D_IntValue(int3 pi)
{
    int A = Noise3D_Perm(pi.x) + pi.y;
    int B = Noise3D_Perm(A) + pi.z;
    int V = Noise3D_Perm(B);
    return V / 255.0f * 2.0f - 1.0f;
}
".NormalizeEndLines();
        }

        public void Perm2D(int px, int py, out int AA, out int AB, out int BA, out int BB)
        {
            int A = Perm(px) + py;
            AA = Perm(A);
            AB = Perm(A + 1);

            int B = Perm(px + 1) + py;
            BA = Perm(B);
            BB = Perm(B + 1);
        }

        public static string Perm2DHlsl()
        {
            return @"
uint4 Noise3D_Perm2D(uint2 p)
{
	return perm2dBuffer.Load(int3(p, 0));
}
".NormalizeEndLines();
        }

        public float GradPerm(int x, Float3 p)
        {
            return Float3.Dot(_g[Perm(x) % 12], p);
        }

        public static string GradPermHlsl()
        {
            return @"
float Noise3D_GradPerm(uint x, float3 p)
{
	return dot(gradPermBuffer.Load(int2(x, 0)).xyz, p);
}
".NormalizeEndLines();
        }

        public float GradientCoherent(float x, float y, float z)
        {
            return GradientCoherent(new Float3(x, y, z));
        }

        public float GradientCoherent(Float3 p)
        {
            int piX = (int)Math.Floor(p.X);
            int piY = (int)Math.Floor(p.Y);
            int piZ = (int)Math.Floor(p.Z);

            p -= Float3.Floor(p);

            Float3 f = Interpolation.SCurve5(p);

            // HASH COORDINATES OF THE 8 CUBE CORNERS
            Perm2D(piX, piY, out int AAx, out int AAy, out int AAz, out int AAw);

            AAx += piZ;
            AAy += piZ;
            AAz += piZ;
            AAw += piZ;

            // AND ADD BLENDED RESULTS FROM 8 CORNERS OF CUBE
            float g1 = GradPerm(AAx, p);
            float g2 = GradPerm(AAz, p + new Float3(-1, 0, 0));
            float g3 = GradPerm(AAy, p + new Float3(0, -1, 0));
            float g4 = GradPerm(AAw, p + new Float3(-1, -1, 0));
            float g5 = GradPerm(AAx + 1, p + new Float3(0, 0, -1));
            float g6 = GradPerm(AAz + 1, p + new Float3(-1, 0, -1));
            float g7 = GradPerm(AAy + 1, p + new Float3(0, -1, -1));
            float g8 = GradPerm(AAw + 1, p + new Float3(-1, -1, -1));

            return Interpolation.Linear(
                Interpolation.Linear(
                    Interpolation.Linear(g1, g2, f.X),
                    Interpolation.Linear(g3, g4, f.X),
                    f.Y),
                Interpolation.Linear(
                    Interpolation.Linear(g5, g6, f.X),
                    Interpolation.Linear(g7, g8, f.X),
                    f.Y),
                f.Z);
        }

        public static string GradientCoherentHlsl()
        {
            return @"
float Noise3D_GradientCoherent(float3 p)
{
	uint3 pi = (uint3)(int3)floor(p) % 256;

	p -= floor(p);

	float3 f = Interpolation_SCurve5(p);

	// HASH COORDINATES OF THE 8 CUBE CORNERS
	uint4 AA = Noise3D_Perm2D(pi.xy) + pi.z;

	// AND ADD BLENDED RESULTS FROM 8 CORNERS OF CUBE
	float g1 = Noise3D_GradPerm(AA.x, p);
	float g2 = Noise3D_GradPerm(AA.z, p + float3(-1, 0, 0));
	float g3 = Noise3D_GradPerm(AA.y, p + float3(0, -1, 0));
	float g4 = Noise3D_GradPerm(AA.w, p + float3(-1, -1, 0));
	float g5 = Noise3D_GradPerm(AA.x + 1, p + float3(0, 0, -1));
	float g6 = Noise3D_GradPerm(AA.z + 1, p + float3(-1, 0, -1));
	float g7 = Noise3D_GradPerm(AA.y + 1, p + float3(0, -1, -1));
	float g8 = Noise3D_GradPerm(AA.w + 1, p + float3(-1, -1, -1));

	float4 x = lerp(float4(g1, g3, g5, g7), float4(g2, g4, g6, g8), f.x);
	float2 y = lerp(x.xz, x.yw, f.y);
	float z = lerp(y.x, y.y, f.z);

	return z;
}
".NormalizeEndLines();
        }
    }
}
