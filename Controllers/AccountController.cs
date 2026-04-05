using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using recrutementapp.Data;
using recrutementapp.Models;
using recrutementapp.ViewModels;

namespace recrutementapp.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Register()
        => User.Identity?.IsAuthenticated == true
            ? RedirectToHome()
            : View(new RegisterViewModel { Role = "Candidate" });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await _db.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "An account with this email already exists.");
            return View(model);
        }

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role = model.Role,
            IsActive = true
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Create profile
        if (model.Role == "Candidate")
            _db.CandidateProfiles.Add(new CandidateProfile { UserId = user.Id });
        else if (model.Role == "Recruiter")
            _db.RecruiterProfiles.Add(new RecruiterProfile { UserId = user.Id });

        await _db.SaveChangesAsync();

        await SignInAsync(user);
        return RedirectToDashboard(user.Role);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return User.Identity?.IsAuthenticated == true ? RedirectToHome() : View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        await SignInAsync(user, model.RememberMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToDashboard(user.Role);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    // ── Helpers ──────────────────────────────────────────────────────────────
    private async Task SignInAsync(User user, bool persistent = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Name),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Role,           user.Role)
        };
        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties { IsPersistent = persistent };
        await HttpContext.SignInAsync("Cookies", principal, properties);
    }

    private IActionResult RedirectToDashboard(string role) => role switch
    {
        "Admin" => RedirectToAction("Index", "Admin"),
        "Recruiter" => RedirectToAction("Index", "Recruiter"),
        _ => RedirectToAction("Dashboard", "Candidate")
    };

    private IActionResult RedirectToHome() => RedirectToAction("Index", "Home");
}