namespace blazor_app.Utils;

public static class OrderDisplay
{
    public static string StatusLabel(string status) => status switch
    {
        "pending" => "Chưa xác nhận",
        "confirmed" => "Đã xác nhận",
        "delivered" => "Đã giao",
        "cancelled" => "Đã hủy",
        _ => status
    };

    public static string StatusClass(string status) => status switch
    {
        "pending" => "text-bg-warning",
        "confirmed" => "text-bg-secondary",
        "delivered" => "text-bg-success",
        "cancelled" => "text-bg-danger",
        _ => "text-bg-secondary"
    };

    public static string PayMethodLabel(string payMethod) => payMethod switch
    {
        "online" => "Thanh toán trực tuyến",
        "cash" => "Thanh toán khi nhận hàng",
        _ => payMethod
    };
}
