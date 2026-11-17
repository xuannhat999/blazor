using blazor_app.Models;

namespace blazor_app.Utils;

public static class ImageUrlHelper
{
    public static string? For(ProductImage? image) => For(image?.Url);

    public static string? For(string? url) =>
        string.IsNullOrWhiteSpace(url) ? null : "/" + url.TrimStart('/');

    public static string? ForFirst(IEnumerable<ProductImage>? images) =>
        For(images?.OrderBy(image => image.SortOrder).FirstOrDefault());
}
