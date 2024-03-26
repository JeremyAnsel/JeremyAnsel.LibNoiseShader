using System;

namespace JeremyAnsel.LibNoiseShader
{
    public sealed class Noise3D
    {
        public Noise3D(int seed)
        {
            this.Seed = seed;
        }

        public int Seed { get; }

        public float IntValue(int piX, int piY, int piZ)
        {
            int seed = 0;
            int n = (1619 * piX + 31337 * piY + 6971 * piZ + 1013 * seed) & 0x7fffffff;
            n = (n >> 13) ^ n;
            n = (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
            return 1.0f - (n / 1073741824.0f);
        }

        public static string IntValueHlsl()
        {
            return @"
float Noise3D_IntValue(int3 pi)
{
    int seed = 0;
    int n = dot(int4(1619, 31337, 6971, 1013), int4(pi, seed)) & 0x7fffffff;
    n = (n >> 13) ^ n;
    n = (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
    return 1.0f - (n / 1073741824.0f);
}
".NormalizeEndLines();
        }

        public float GradientCoherent(float x, float y, float z)
        {
            return GradientCoherent(new Float3(x, y, z));
        }

        private static Float3 GradientCoherentRand3(Float3 c)
        {
            float j = (float)Math.Sin(Float3.Dot(c, new Float3(17.0f, 59.4f, 15.0f)));
            Float3 r = new Float3(262144.0f, 32768.0f, 2097152.0f) * j;
            r *= 1.0f / 262144.0f;
            r -= Float3.Floor(r);
            return r - 0.5f;
        }

        public float GradientCoherent(Float3 p)
        {
            Float3 s = Float3.Floor(p + Float3.Dot(p, new Float3(0.3333333f, 0.3333333f, 0.3333333f)));
            Float3 x = p - s + Float3.Dot(s, new Float3(0.1666667f, 0.1666667f, 0.1666667f));

            Float3 e = Float3.Step(new Float3(0.0f, 0.0f, 0.0f), x - new Float3(x.Y, x.Z, x.X));
            Float3 i1 = e * (1.0f - new Float3(e.Z, e.X, e.Y));
            Float3 i2 = 1.0f - new Float3(e.Z, e.X, e.Y) * (1.0f - e);

            Float3 x1 = x - i1 + 0.1666667f;
            Float3 x2 = x - i2 + 2.0f * 0.1666667f;
            Float3 x3 = x - 1.0f + 3.0f * 0.1666667f;

            Float4 w, d;

            w.X = Float3.Dot(x, x);
            w.Y = Float3.Dot(x1, x1);
            w.Z = Float3.Dot(x2, x2);
            w.W = Float3.Dot(x3, x3);

            w = Float4.Max(0.6f - w, 0.0f);

            d.X = Float3.Dot(GradientCoherentRand3(s), x);
            d.Y = Float3.Dot(GradientCoherentRand3(s + i1), x1);
            d.Z = Float3.Dot(GradientCoherentRand3(s + i2), x2);
            d.W = Float3.Dot(GradientCoherentRand3(s + 1.0f), x3);

            w *= w;
            w *= w;
            d *= w;

            return Float4.Dot(d, new Float4(52.0f, 52.0f, 52.0f, 52.0f));
        }

        public static string GradientCoherentHlsl()
        {
            return @"
float3 Noise3D_GradientCoherent_rand3(float3 c)
{
    float j = sin(dot(c, float3(17.0, 59.4, 15.0)));
    float3 r = float3(262144.0, 32768.0, 2097152.0) * j;
    r *= 1.0 / 262144.0;
    r -= floor(r);
    return r - 0.5;
}

float Noise3D_GradientCoherent(float3 p)
{
    float3 s = floor(p + dot(p, float3(0.3333333, 0.3333333, 0.3333333)));
    float3 x = p - s + dot(s, float3(0.1666667, 0.1666667, 0.1666667));
	 
    float3 e = step(float3(0.0, 0.0, 0.0), x - x.yzx);
    float3 i1 = e * (1.0 - e.zxy);
    float3 i2 = 1.0 - e.zxy * (1.0 - e);
	 	
    float3 x1 = x - i1 + 0.1666667;
    float3 x2 = x - i2 + 2.0 * 0.1666667;
    float3 x3 = x - 1.0 + 3.0 * 0.1666667;
	 
    float4 w, d;
	 
    w.x = dot(x, x);
    w.y = dot(x1, x1);
    w.z = dot(x2, x2);
    w.w = dot(x3, x3);

    w = max(0.6 - w, 0.0);
	 
    d.x = dot(Noise3D_GradientCoherent_rand3(s), x);
    d.y = dot(Noise3D_GradientCoherent_rand3(s + i1), x1);
    d.z = dot(Noise3D_GradientCoherent_rand3(s + i2), x2);
    d.w = dot(Noise3D_GradientCoherent_rand3(s + 1.0), x3);
	 
    w *= w;
    w *= w;
    d *= w;
	 
    return dot(d, float4(52.0, 52.0, 52.0, 52.0));
}
".NormalizeEndLines();
        }
    }
}
