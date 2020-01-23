using System;

namespace PaatyDSM
{
    public static class Cpp_Functions
    {
        public static int? FindFirstNotOf(this string source, string chars)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (chars == null) throw new ArgumentNullException(nameof(chars));
            if (source.Length == 0) return null;
            if (chars.Length == 0) return 0;

            for (int i = 0; i < source.Length; i++)
            {
                if (chars.IndexOf(source[i]) == -1) return i;
            }
            return -1;
        }

        public static int? FindLastNotOf(this string source, string chars)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (chars == null) throw new ArgumentNullException(nameof(chars));
            if (source.Length == 0) return null;
            if (chars.Length == 0) return source.Length - 1;

            for (int i = source.Length - 1; i >= 0; i--)
            {
                if (chars.IndexOf(source[i]) == -1) return i;
            }
            return null;
        }
    }
}
