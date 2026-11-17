using System.Security.Claims;
using blazor_app.Interfaces;
using blazor_app.Models;
using Microsoft.AspNetCore.Mvc;

namespace blazor_app.Controllers;

[Route("cart")]
public class CartController : Controller
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> Add()
    {
        var form = await Request.ReadFormAsync();

        if (HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return Redirect("/login");
        }

        if (!Enum.TryParse<Size>(form["size"].ToString().Trim(), true, out var size))
        {
            size = Size.M;
        }

        _ = int.TryParse(form["productId"].ToString(), out var productId);
        _ = int.TryParse(form["quantity"].ToString(), out var quantity);

        if (quantity <= 0)
        {
            quantity = 1;
        }

        var userIdValue = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Redirect("/login");
        }

        await _cartService.AddAsync(userId, productId, size, quantity);

        return Redirect($"/products/{productId}?added=1");
    }
}
