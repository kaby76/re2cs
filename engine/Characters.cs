using System.Globalization;

namespace engine
{

    /** Wraps Character methods to be overridden for GWT. */
    public sealed class Characters
    {
        public static int toLowerCase(int codePoint)
        {
            return System.Char.ToLower((char)codePoint, CultureInfo.CurrentCulture);
        }

        public static int toUpperCase(int codePoint)
        {
            return System.Char.ToUpper((char)codePoint, CultureInfo.CurrentCulture);
        }
    }
}