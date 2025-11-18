using LoginandRegisterMVC.Data;
using LoginandRegisterMVC.Models;
using LoginandRegisterMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LoginandRegisterMVC.Controllers;

public class UsersController(UserContext context, IPasswordHashService passwordHashService) : Controller
{
    private readonly UserContext _context = context;
    private readonly IPasswordHashService _passwordHashService = passwordHashService;

    // GET: Users
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    public IActionResult Register()
    {
        return View(new User());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User user)
    {
        var existingUser = await _context.Users
            .Where(u => u.UserId.Equals(user.UserId))
            .FirstOrDefaultAsync();

        if (existingUser == null)
        {
            if (ModelState.IsValid)
            {
                user.Password = _passwordHashService.HashPassword(user.Password);
                user.ConfirmPassword = _passwordHashService.HashPassword(user.ConfirmPassword);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Error Occurred! Try again!!");
            }
        }
        else
        {
            ModelState.AddModelError("", "User exists, Please login with your password");
        }
        return View(user);
    }

    public async Task<IActionResult> Login()
    {
        var adminUser = await _context.Users
            .Where(u => u.UserId.Equals("admin@demo.com"))
            .FirstOrDefaultAsync();

        if (adminUser == null)
        {
            var user = new User
            {
                UserId = "admin@demo.com",
                Username = "admin",
                Password = _passwordHashService.HashPassword("Admin@123"),
                ConfirmPassword = _passwordHashService.HashPassword("Admin@123"),
                Role = "Admin"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        return View(new User());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(User user)
    {
        var hashedPassword = _passwordHashService.HashPassword(user.Password);

        var authenticatedUser = await _context.Users
            .Where(u => u.UserId.Equals(user.UserId) && u.Password.Equals(hashedPassword))
            .FirstOrDefaultAsync();

        if (authenticatedUser != null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, authenticatedUser.UserId),
                new(ClaimTypes.NameIdentifier, authenticatedUser.UserId),
                new("Username", authenticatedUser.Username),
                new(ClaimTypes.Role, authenticatedUser.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            HttpContext.Session.SetString("UserId", authenticatedUser.UserId);
            HttpContext.Session.SetString("Username", authenticatedUser.Username);
            HttpContext.Session.SetString("Role", authenticatedUser.Role);

            return RedirectToAction("Index");
        }
        else
        {
            ModelState.AddModelError("", "UserId or password wrong");
        }

        return View(user);
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
