using Microsoft.AspNetCore.Mvc;
using Planner.Models;
using System.Text;
using System;
using Planner.Data;
using System.Security.Cryptography;

namespace Planner.Controllers
{
    public class AuthController : Controller
    {
        private readonly PlannerDbContext _dbContext;

        public AuthController(PlannerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(User user)
        {
            // if (ModelState.IsValid)
            // {
                user.Password = HashPassword(user.Password); // Hash the password
                user.Role = UserRole.OrdinaryUser; // Default role for new users
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges();

                return RedirectToAction("Login");
            // }

            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, bool rememberMe)
        {
            var hashedPassword = HashPassword(password);
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email && u.Password == hashedPassword);

            if (user != null)
            {
                // Generate and set cookies
                var token = GenerateToken();
                var expirationTime = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1);

                // Add or update session
                var session = _dbContext.Sessions.FirstOrDefault(s => s.UserId == user.Id);

                if (session == null)
                {
                    session = new Session { UserId = user.Id, SessionToken = token, ExpirationTime = expirationTime };
                    _dbContext.Sessions.Add(session);
                }
                else
                {
                    session.SessionToken = token;
                    session.ExpirationTime = expirationTime;
                    _dbContext.Sessions.Update(session);
                }

                _dbContext.SaveChanges();

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/",
                    Expires = expirationTime
                };

                Response.Cookies.Append("session_id", token, cookieOptions);
                Response.Cookies.Append("user_role", user.Role.ToString(), cookieOptions);

                return Redirect("/");
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials");
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("session_id");
            Response.Cookies.Delete("user_role");

            return RedirectToAction("Login");
        }

       

        // Utility methods

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private string GenerateToken()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 32).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool IsAuthenticated()
        {
            return Request.Cookies.ContainsKey("session_id");
        }

        
    }
}