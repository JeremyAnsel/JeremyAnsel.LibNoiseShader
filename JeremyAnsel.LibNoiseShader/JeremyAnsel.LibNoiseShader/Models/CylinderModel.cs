using System;

namespace JeremyAnsel.LibNoiseShader.Models
{
    public static class CylinderModel
    {
        public static Float3 GetCoords(float angle, float height)
        {
            float angleRad = angle * (float)Math.PI / 180.0f;

            Float3 result;
            result.X = (float)Math.Cos(angleRad);
            result.Y = height;
            result.Z = (float)Math.Sin(angleRad);
            return result;
        }

        public static string GetHlsl()
        {
            return @"
float3 Model_Cylinder(float angle, float height)
{
    float angleRad = radians(angle);

    float angleSin;
    float angleCos;
    sincos(angleRad, angleSin, angleCos);

    float3 result;
    result.x = angleCos;
    result.y = height;
    result.z = angleSin;
    return result;
}
".NormalizeEndLines();
        }
    }
}
