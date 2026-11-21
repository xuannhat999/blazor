using System.Security.Claims;
using blazor_app.Interfaces;
using blazor_app.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace blazor_app.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update()
    {
        if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Redirect("/login");
        }

        var form = await Request.ReadFormAsync();

        var name = form["name"].ToString().Trim();
        var address = form["address"].ToString().Trim();
        var payMethodValue = form["payMethod"].ToString().Trim();

        if (name.Length == 0 || address.Length == 0)
        {
            return Redirect("/account?error=missing");
        }

        if (!Enum.TryParse<PayMethod>(payMethodValue, true, out var payMethod))
        {
            return Redirect("/account?error=missing");
        }

        var updated = await _userService.UpdateProfileAsync(userId, name, address, payMethod);

        if (!updated)
        {
            return Redirect("/account?error=missing");
        }

        var user = await _userService.GetByIdAsync(userId);

        if (user is not null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true });
        }

        return Redirect("/account?updated=1");
    }
}
