using System.Security.Claims;
using KulturSanatPortal.Web.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KulturSanatPortal.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AuthController(IAdminAuth auth) : Controller
{
    // GET: /admin/login?returnUrl=/Admin
    [HttpGet("/admin/login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl; // view hidden alanına koyacağız
        return View();
    }

    // POST: /admin/login
    [HttpPost("/admin/login")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        if (auth.Validate(username, password))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("role", "admin")
            };
            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(id));

            // open-redirect'e karşı güvenli dönüş
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        ViewBag.Error = "Kullanıcı adı veya şifre yanlış.";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST: /admin/logout
    [HttpPost("/admin/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/admin/login");
    }
}
