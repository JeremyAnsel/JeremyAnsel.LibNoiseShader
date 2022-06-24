namespace JeremyAnsel.LibNoiseShader
{
    internal static class StringExtensions
    {
        public static string NormalizeEndLines(this string s)
        {
            return s
                .Replace("\r\n", "\n")
                .Replace("\n", "\r\n");
        }
    }
}
