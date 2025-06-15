using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVPDS.Entities;
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
    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login() => View();
    
    [HttpGet("register")]
    [AllowAnonymous]
    public IActionResult Register() => View();
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _db.VoiceUsers
            .FirstOrDefaultAsync(u => u.Username == username && u.UserPassword == password);

        if (user == null)
            return Unauthorized("Неверный логин или пароль");

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
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(string username, string password)
    {
        if (await _db.VoiceUsers.AnyAsync(u => u.Username == username))
            return BadRequest("Пользователь уже существует");

        var user = new VoiceUser
        {
            Username = username,
            UserPassword = password
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