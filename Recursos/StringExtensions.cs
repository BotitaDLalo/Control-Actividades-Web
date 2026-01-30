using System;

namespace ControlActividades.Recursos
{
    /// <summary>
    /// Helper extension methods for safe, culture-invariant string comparisons.
    /// Use these instead of calling `ToLower()` for comparisons.
    /// </summary>
    public static class StringExtensions
    {
        public static bool EqualsInvariantIgnoreCase(this string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsInvariantIgnoreCase(this string source, string toCheck)
        {
            if (source == null || toCheck == null) return false;
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool StartsWithInvariantIgnoreCase(this string source, string value)
        {
            if (source == null || value == null) return false;
            return source.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
