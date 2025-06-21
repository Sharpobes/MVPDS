using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVPDS.Entities;
using MVPDS.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[AllowAnonymous]
[Route("[controller]")]
public class AuthController : Controller
{
    private readonly MvpdsContext _db;

    public AuthController(MvpdsContext db)
    {
        _db = db;
    }

    [HttpGet("login")]
    public IActionResult Login() => View();

    [HttpGet("register")]
    public IActionResult Register() => View();

    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _db.VoiceUsers.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            TempData["LoginError"] = "Неверный логин или пароль";
            return RedirectToAction("Login");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("UserId", user.VoiceUsersId.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Servers", "Voice");
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Auth");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string username, string password)
    {
        if (await _db.VoiceUsers.AnyAsync(u => u.Username == username))
        {
            TempData["RegisterError"] = "Пользователь уже существует";
            return RedirectToAction("Register");
        }
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            TempData["RegisterError"] = "Пароль должен содержать минимум 6 символов.";
            return RedirectToAction("Register");
        }
        var hashedPassword = PasswordHasher.HashPassword(password);

        var user = new VoiceUser
        {
            Username = username,
            UserPassword = hashedPassword
        };

        _db.VoiceUsers.Add(user);
        await _db.SaveChangesAsync();

        return RedirectToAction("Login", "Auth");
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var username = User.Identity?.Name;
        var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        return Ok(new { username, userId });
    }
}
