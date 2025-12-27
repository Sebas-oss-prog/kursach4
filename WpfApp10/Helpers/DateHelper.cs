using System;
using System.Globalization;

namespace WpfApp10.Helpers
{
    public static class DateHelper
    {
        public static DateTime? Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;

            if (DateTime.TryParseExact(
                s,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var d))
                return d;

            return null;
        }
    }
}
