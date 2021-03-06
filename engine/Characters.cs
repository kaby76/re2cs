﻿using System;
using System.Globalization;

namespace engine
{

    /** Wraps Character methods to be overridden for GWT. */
    public sealed class Characters
    {
        public static int toLowerCase(int codePoint)
        {
            // Convert UTF-32 character to a UTF-16 String.
            var strC = Char.ConvertFromUtf32(codePoint);

            // Casing rules depends on the culture.
            // Consider using ToLowerInvariant().
            var lower = strC.ToLower(CultureInfo.InvariantCulture);

            // Convert the UTF-16 String back to UTF-32 character and return it.
            return Char.ConvertToUtf32(lower, 0);
        }

        public static int toUpperCase(int codePoint)
        {
            // Convert UTF-32 character to a UTF-16 String.
            var strC = Char.ConvertFromUtf32(codePoint);

            // Casing rules depends on the culture.
            // Consider using ToLowerInvariant().
            var lower = strC.ToUpper(CultureInfo.InvariantCulture);

            // Convert the UTF-16 String back to UTF-32 character and return it.
            return Char.ConvertToUtf32(lower, 0);
        }
    }
}