namespace blazor_app.Utils;

public static class ProductDisplay
{
    public static int? DiscountPercent(decimal price, decimal? oldPrice)
    {
        if (oldPrice is not decimal oldPriceValue || oldPriceValue <= price || oldPriceValue <= 0)
        {
            return null;
        }

        return (int)Math.Round((1 - price / oldPriceValue) * 100m);
    }
}
