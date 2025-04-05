using Microsoft.AspNetCore.Mvc;
using Planner.Data;
using Planner.Models;
using System;
using System.Diagnostics;

namespace Planner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly PlannerDbContext _dbContext;

        public HomeController(PlannerDbContext dbContext, ILogger<HomeController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Fetch user ID from cookies or session (example placeholder logic)
            if (!Request.Cookies.TryGetValue("session_id", out var token))
            {
                return Redirect("/Auth/Login"); // Redirect to login if no session cookie
            }

            string userRole = GetUserRole();
            if(userRole == "Admin")
            {
                var users = _dbContext.Users.ToList();
                return View("AdminDashboard", users);
            }

            // Temporary: Simulate fetching user ID from session (replace with actual authentication logic)
            var userId = GetUserIdFromSession();
            if (userId == 0) return Redirect("/Auth/Login");

            // Fetch tasks for the user from the database
            var tasks = _dbContext.Tasks.Where(t => t.UserId == userId).ToList();

            return View(tasks); // Pass tasks as the model to the view
        }

        private int GetUserIdFromSession()
        {
            if (Request.Cookies.TryGetValue("session_id", out var token))
            {
                var session = _dbContext.Sessions.FirstOrDefault(s => s.SessionToken == token && s.ExpirationTime > DateTime.UtcNow);
                return session?.UserId ?? 0;
            }

            return 0; // Return 0 if no valid session is found
        }

        private string GetUserRole()
        {
            return Request.Cookies["user_role"];
        }

    }

}
