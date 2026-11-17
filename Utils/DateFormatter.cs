using System.Globalization;

namespace blazor_app.Utils;

public static class DateFormatter
{
    public static string Format(DateTime value) => value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
}
