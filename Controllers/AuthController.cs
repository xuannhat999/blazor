using System.Security.Claims;
using blazor_app.Interfaces;
using blazor_app.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace blazor_app.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        var form = await Request.ReadFormAsync();
        var email = form["email"].ToString();
        var password = form["password"].ToString();

        var user = await _userService.AuthenticateAsync(email, password);

        if (user is null)
        {
            return Redirect("/login?error=1");
        }

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

        return Redirect("/");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register()
    {
        var form = await Request.ReadFormAsync();

        var name = form["name"].ToString().Trim();
        var email = form["email"].ToString().Trim();
        var password = form["password"].ToString();
        var address = form["address"].ToString().Trim();
        var payMethodValue = form["payMethod"].ToString().Trim();

        if (name.Length == 0
            || email.Length == 0
            || password.Length == 0
            || address.Length == 0)
        {
            return Redirect("/register?error=missing");
        }

        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
        {
            return Redirect("/register?error=missing");
        }

        if (!Enum.TryParse<PayMethod>(payMethodValue, true, out var payMethod))
        {
            if (payMethodValue.Length > 0)
            {
                return Redirect("/register?error=missing");
            }

            payMethod = PayMethod.Cash;
        }

        var user = new User
        {
            Name = name,
            Email = email,
            Password = password,
            Address = address,
            PayMethod = payMethod,
            Role = UserRole.Buyer,
            IsLocked = false
        };

        var created = await _userService.RegisterAsync(user);

        if (!created)
        {
            return Redirect("/register?error=email");
        }

        return Redirect("/login?registered=1");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
