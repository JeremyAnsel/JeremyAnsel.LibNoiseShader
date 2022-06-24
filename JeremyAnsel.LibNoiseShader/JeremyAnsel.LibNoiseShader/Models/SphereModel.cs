namespace JeremyAnsel.LibNoiseShader.Models
{
    public static class SphereModel
    {
        public static Float3 GetCoords(float lat, float lon)
        {
            return LatLon.ToXYZ(lat, lon);
        }

        public static string GetHlsl()
        {
            return @"
float3 Model_Sphere(float lat, float lon)
{
    return LatLon_ToXYZ(lat, lon);
}
".NormalizeEndLines();
        }
    }
}
