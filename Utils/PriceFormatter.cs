using System.Globalization;

namespace blazor_app.Utils;

public static class PriceFormatter
{
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");

    public static string Format(decimal value) => value.ToString("N0", VietnameseCulture) + "₫";
}
