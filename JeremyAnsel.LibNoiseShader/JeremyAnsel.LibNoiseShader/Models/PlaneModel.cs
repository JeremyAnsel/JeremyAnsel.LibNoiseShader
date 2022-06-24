namespace JeremyAnsel.LibNoiseShader.Models
{
    public static class PlaneModel
    {
        public static Float3 GetCoords(float x, float y)
        {
            return new Float3(x, y, 0.0f);
        }

        public static string GetHlsl()
        {
            return @"
float3 Model_Plane(float x, float y)
{
    return float3(x, y, 0.0f);
}
".NormalizeEndLines();
        }
    }
}
