using System;

namespace JeremyAnsel.LibNoiseShader
{
    public static class LatLon
    {
        public static Float3 ToXYZ(float lat, float lon)
        {
            float latRad = lat * (float)Math.PI / 180.0f;
            float lonRad = lon * (float)Math.PI / 180.0f;

            float r = (float)Math.Cos(latRad);

            Float3 result;
            result.X = r * (float)Math.Cos(lonRad);
            result.Y = (float)Math.Sin(latRad);
            result.Z = r * (float)Math.Sin(lonRad);
            return result;
        }

        public static string ToXYZHlsl()
        {
            return @"
float3 LatLon_ToXYZ(float lat, float lon)
{
    float latRad = radians(lat);
    float lonRad = radians(lon);

    float latS;
    float latC;
    sincos(latRad, latS, latC);

    float lonS;
    float lonC;
    sincos(lonRad, lonS, lonC);

    float3 result;
    result.x = latC * lonC;
    result.y = latS;
    result.z = latC * lonS;
    return result;
}
".NormalizeEndLines();
        }
    }
}
