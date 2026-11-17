using System.Security.Claims;
using blazor_app.Interfaces;
using blazor_app.Models;
using Microsoft.AspNetCore.Mvc;

namespace blazor_app.Controllers;

[Route("orders")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create()
    {
        if (HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return Redirect("/login");
        }

        var userIdValue = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Redirect("/login");
        }

        var form = await Request.ReadFormAsync();

        var ids = form["ids"]
            .Select(value => int.TryParse(value, out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var address = form["address"].ToString().Trim();

        var payMethod = Enum.TryParse<PayMethod>(form["payMethod"].ToString().Trim(), true, out var pm)
            ? pm
            : PayMethod.Cash;

        if (address.Length == 0)
        {
            return Redirect("/payment?error=address&" + string.Join("&", ids.Select(id => "ids=" + id)));
        }

        if (ids.Count == 0)
        {
            return Redirect("/cart");
        }

        var order = await _orderService.CreateFromCartAsync(userId, ids, address, payMethod);

        if (order is null)
        {
            return Redirect("/cart");
        }

        return Redirect("/payment?done=1");
    }
}
