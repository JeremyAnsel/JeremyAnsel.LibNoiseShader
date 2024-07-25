using System;
using System.Globalization;
using System.Linq;

namespace JeremyAnsel.LibNoiseShader
{
    internal sealed class FloatNumberFormat : IFormatProvider, ICustomFormatter
    {
        public static readonly FloatNumberFormat Default = new();

        public object? GetFormat(Type? formatType)
        {
            if (formatType == typeof(ICustomFormatter))
            {
                return this;
            }

            return null;
        }

        public string Format(string? fmt, object? arg, IFormatProvider? formatProvider)
        {
            if (arg is float f)
            {
                string result = f.ToString(CultureInfo.InvariantCulture);

                if (result.Contains('.'))
                {
                    result += "f";
                }

                return result;
            }

            try
            {
                return HandleOtherFormats(fmt, arg);
            }
            catch (FormatException e)
            {
                throw new FormatException(string.Format("The format of '{0}' is invalid.", fmt), e);
            }
        }

        private static string HandleOtherFormats(string? format, object? arg)
        {
            if (arg is IFormattable formattable)
            {
                return formattable.ToString(format, CultureInfo.InvariantCulture);
            }

            if (arg != null)
            {
                return arg.ToString()!;
            }

            return string.Empty;
        }
    }
}
