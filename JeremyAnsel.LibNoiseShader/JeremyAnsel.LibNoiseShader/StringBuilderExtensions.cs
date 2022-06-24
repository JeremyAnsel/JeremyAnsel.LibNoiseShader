using System.Text;

namespace JeremyAnsel.LibNoiseShader
{
    internal static class StringBuilderExtensions
    {
        public static void AppendTabFormatLine(this StringBuilder sb)
        {
            sb.AppendLine();
        }

        public static void AppendTabFormatLine(this StringBuilder sb, int tab, string format)
        {
            if (tab > 0)
            {
                sb.Append(new string(' ', tab * 4));
            }

            sb.Append(format);
            sb.AppendLine();
        }

        public static void AppendTabFormatLine(this StringBuilder sb, string format)
        {
            sb.AppendTabFormatLine(0, format);
        }

        public static void AppendTabFormatLine(this StringBuilder sb, int tab, string format, params object[] args)
        {
            if (tab > 0)
            {
                sb.Append(new string(' ', tab * 4));
            }

            sb.AppendFormat(FloatNumberFormat.Default, format, args);
            sb.AppendLine();
        }

        public static void AppendTabFormatLine(this StringBuilder sb, string format, params object[] args)
        {
            sb.AppendTabFormatLine(0, format, args);
        }

        public static void AppendTabFormatLine(this StringBuilder sb, int tab, string format, object arg0)
        {
            if (tab > 0)
            {
                sb.Append(new string(' ', tab * 4));
            }

            sb.AppendFormat(FloatNumberFormat.Default, format, arg0);
            sb.AppendLine();
        }

        public static void AppendTabFormatLine(this StringBuilder sb, string format, object arg0)
        {
            sb.AppendTabFormatLine(0, format, arg0);
        }

        public static void AppendTabFormatLine(this StringBuilder sb, int tab, string format, object arg0, object arg1)
        {
            if (tab > 0)
            {
                sb.Append(new string(' ', tab * 4));
            }

            sb.AppendFormat(FloatNumberFormat.Default, format, arg0, arg1);
            sb.AppendLine();
        }

        public static void AppendTabFormatLine(this StringBuilder sb, string format, object arg0, object arg1)
        {
            sb.AppendTabFormatLine(0, format, arg0, arg1);
        }

        public static void AppendTabFormatLine(this StringBuilder sb, int tab, string format, object arg0, object arg1, object arg2)
        {
            if (tab > 0)
            {
                sb.Append(new string(' ', tab * 4));
            }

            sb.AppendFormat(FloatNumberFormat.Default, format, arg0, arg1, arg2);
            sb.AppendLine();
        }

        public static void AppendTabFormatLine(this StringBuilder sb, string format, object arg0, object arg1, object arg2)
        {
            sb.AppendTabFormatLine(0, format, arg0, arg1, arg2);
        }
    }
}
