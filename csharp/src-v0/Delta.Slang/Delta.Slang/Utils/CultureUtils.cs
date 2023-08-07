using System;
using System.Globalization;

namespace Delta.Slang.Utils;

public static class CultureUtils
{
    private sealed class TemporaryCulture : IDisposable
    {
        private readonly CultureInfo previousCulture;
        private readonly CultureInfo previousUICulture;

        public TemporaryCulture(CultureInfo culture)
        {
            previousCulture = CultureInfo.CurrentCulture;
            previousUICulture = CultureInfo.CurrentUICulture;

            CultureInfo.CurrentCulture = culture ?? throw new ArgumentNullException(nameof(culture));
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUICulture;
        }
    }

    public static IDisposable InvariantCulture() => new TemporaryCulture(CultureInfo.InvariantCulture);
}
