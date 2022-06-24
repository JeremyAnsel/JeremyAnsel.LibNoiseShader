namespace JeremyAnsel.LibNoiseShader
{
    public static class Interpolation
    {
        public static float Cubic(float n0, float n1, float n2, float n3, float a)
        {
            float p = (n3 - n2) - (n0 - n1);
            float q = (n0 - n1) - p;
            float r = n2 - n0;
            float s = n1;
            return (p * a * a * a) + (q * a * a) + (r * a) + s;
        }

        public static Float3 Cubic(Float3 n0, Float3 n1, Float3 n2, Float3 n3, Float3 a)
        {
            Float3 p = (n3 - n2) - (n0 - n1);
            Float3 q = (n0 - n1) - p;
            Float3 r = n2 - n0;
            Float3 s = n1;
            return (p * a * a * a) + (q * a * a) + (r * a) + s;
        }

        public static string CubicHlsl()
        {
            return @"
float Interpolation_Cubic(float n0, float n1, float n2, float n3, float a)
{
    float p = (n3 - n2) - (n0 - n1);
    float q = (n0 - n1) - p;
    float r = n2 - n0;
    float s = n1;
    return (p * a * a * a) + (q * a * a) + (r * a) + s;
}

float3 Interpolation_Cubic(float3 n0, float3 n1, float3 n2, float3 n3, float3 a)
{
    float3 p = (n3 - n2) - (n0 - n1);
    float3 q = (n0 - n1) - p;
    float3 r = n2 - n0;
    float3 s = n1;
    return (p * a * a * a) + (q * a * a) + (r * a) + s;
}
".NormalizeEndLines();
        }

        public static byte Linear(byte n0, byte n1, byte a)
        {
            return (byte)(Linear(n0 / 255.0f, n1 / 255.0f, a / 255.0f) * 255.0f);
        }

        public static float Linear(float n0, float n1, float a)
        {
            return n0 + a * (n1 - n0);
        }

        public static Float3 Linear(Float3 n0, Float3 n1, Float3 a)
        {
            return n0 + a * (n1 - n0);
        }

        public static string LinearHlsl()
        {
            return @"
float Interpolation_Linear(float n0, float n1, float a)
{
    return n0 + a * (n1 - n0);
}

float3 Interpolation_Linear(float3 n0, float3 n1, float3 a)
{
    return n0 + a * (n1 - n0);
}
".NormalizeEndLines();
        }

        public static float SCurve3(float a)
        {
            return a * a * (3.0f - (2.0f * a));
        }

        public static Float3 SCurve3(Float3 a)
        {
            return a * a * (3.0f - (2.0f * a));
        }

        public static string SCurve3Hlsl()
        {
            return @"
float Interpolation_SCurve3(float a)
{
    return a * a * (3.0f - (2.0f * a));
}

float3 Interpolation_SCurve3(float3 a)
{
    return a * a * (3.0f - (2.0f * a));
}
".NormalizeEndLines();
        }

        public static float SCurve5(float a)
        {
            return a * a * a * (a * (a * 6.0f - 15.0f) + 10.0f);
        }

        public static Float3 SCurve5(Float3 a)
        {
            return a * a * a * (a * (a * 6.0f - 15.0f) + 10.0f);
        }

        public static string SCurve5Hlsl()
        {
            return @"
float Interpolation_SCurve5(float a)
{
    return a * a * a * (a * (a * 6.0f - 15.0f) + 10.0f);
}

float3 Interpolation_SCurve5(float3 a)
{
    return a * a * a * (a * (a * 6.0f - 15.0f) + 10.0f);
}
".NormalizeEndLines();
        }
    }
}
