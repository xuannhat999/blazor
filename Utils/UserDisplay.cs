using blazor_app.Models;

namespace blazor_app.Utils;

public static class UserDisplay
{
    public static string RoleLabel(UserRole role) => role switch
    {
        UserRole.Admin => "Quản trị viên",
        UserRole.Buyer => "Khách hàng",
        _ => role.ToString()
    };
}
