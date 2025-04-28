using Microsoft.AspNetCore.Mvc;
using Planner.Models;
using System.Text;
using System;
using Planner.Data;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace Planner.Controllers
{
    public class AuthController : Controller
    {
        private readonly PlannerDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;

        public AuthController(PlannerDbContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
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

        //[HttpPost]
        //public IActionResult Login(string email, string password, bool rememberMe)
        //{
        //    string cacheKey = $"FailedLogin_{email.ToLower()}";

        //    _memoryCache.TryGetValue(cacheKey, out int attempts);

        //    if (attempts >= 5)
        //    {
        //        ModelState.AddModelError(string.Empty, "Too many failed login attempts. Please try again later.");
        //        return View();
        //    }

        //    var hashedPassword = HashPassword(password);
        //    var user = _dbContext.Users.FirstOrDefault(u => u.Email == email && u.Password == hashedPassword);

        //    if (user != null)
        //    {
        //        _memoryCache.Remove(cacheKey);
        //        // Generate and set cookies
        //        var token = GenerateToken();
        //        var expirationTime = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1);

        //        // Add or update session
        //        var session = _dbContext.Sessions.FirstOrDefault(s => s.UserId == user.Id);

        //        if (session == null)
        //        {
        //            session = new Session { UserId = user.Id, SessionToken = token, ExpirationTime = expirationTime };
        //            _dbContext.Sessions.Add(session);
        //        }
        //        else
        //        {
        //            session.SessionToken = token;
        //            session.ExpirationTime = expirationTime;
        //            _dbContext.Sessions.Update(session);
        //        }

        //        _dbContext.SaveChanges();

        //        var cookieOptions = new CookieOptions
        //        {
        //            HttpOnly = true,
        //            Secure = true,
        //            SameSite = SameSiteMode.Strict,
        //            Path = "/",
        //            Expires = expirationTime
        //        };

        //        Response.Cookies.Append("session_id", token, cookieOptions);
        //        Response.Cookies.Append("user_role", user.Role.ToString(), cookieOptions);

        //        return Redirect("/");
        //    }
        //    attempts++;


        //    _memoryCache.Set(cacheKey, attempts, new MemoryCacheEntryOptions
        //    {
        //        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        //    });

        //    ModelState.AddModelError(string.Empty, "Invalid login credentials");
        //    return View();
        //}

        [HttpPost]
        public IActionResult Login(string email, string password, bool rememberMe)
        {
            string userKey = $"FailedLogin_User_{email.ToLower()}";
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            string ipKey = $"FailedLogin_IP_{ip}";

            string userLockKey = $"Lockout_User_{email.ToLower()}";
            string ipLockKey = $"Lockout_IP_{ip}";

           
            bool isUserLocked = _memoryCache.TryGetValue(userLockKey, out DateTime userLockUntil) && DateTime.UtcNow < userLockUntil;
            bool isIpLocked = _memoryCache.TryGetValue(ipLockKey, out DateTime ipLockUntil) && DateTime.UtcNow < ipLockUntil;

            if (isUserLocked)
            {
                ViewBag.Locked = true;
                ViewBag.LockReason = $"Too many failed attempts for this user. Try again at {userLockUntil.ToLocalTime():HH:mm:ss}.";
                return View();
            }
            if (isIpLocked)
            {
                ViewBag.Locked = true;
                ViewBag.LockReason = $"Too many failed attempts from your IP address. Try again at {ipLockUntil.ToLocalTime():HH:mm:ss}.";
                return View();
            }

            ViewBag.Locked = false;

            int userAttempts = _memoryCache.GetOrCreate(userKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return 0;
            });

            int ipAttempts = _memoryCache.GetOrCreate(ipKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return 0;
            });

            if (userAttempts >= 4)
            {
                var lockExpiration = DateTime.UtcNow.AddMinutes(1);
                _memoryCache.Set(userLockKey, lockExpiration, TimeSpan.FromMinutes(1)); 
                ViewBag.Locked = true;
                ViewBag.LockReason = $"Too many failed attempts for this user. Try again at {lockExpiration.ToLocalTime():HH:mm:ss}.";
                return View();
            }
            if (ipAttempts >= 9)
            {
                var lockExpiration = DateTime.UtcNow.AddMinutes(15);
                _memoryCache.Set(ipLockKey, lockExpiration, TimeSpan.FromMinutes(15));
                ViewBag.Locked = true;
                ViewBag.LockReason = $"Too many failed attempts from your IP address. Try again at {lockExpiration.ToLocalTime():HH:mm:ss}.";
                return View();
            }

            var hashedPassword = HashPassword(password);
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email && u.Password == hashedPassword);

            if (user != null)
            {
        
                _memoryCache.Remove(userKey);
                _memoryCache.Remove(ipKey);
                _memoryCache.Remove(userLockKey);
                _memoryCache.Remove(ipLockKey);

                var token = GenerateToken();
                var expirationTime = rememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1);

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

            
            _memoryCache.Set(userKey, userAttempts + 1, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            });
            _memoryCache.Set(ipKey, ipAttempts + 1, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            });

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