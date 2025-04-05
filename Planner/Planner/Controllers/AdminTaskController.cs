using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Planner.Data;
using Planner.Models;

namespace Planner.Controllers
{
	public class AdminTaskController : Controller
	{
		private readonly PlannerDbContext _dbContext;
		public AdminTaskController(PlannerDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<IActionResult> Index()
		{
			var tasks = await _dbContext.Tasks.Include(t => t.User).ToListAsync();
			return View(tasks);
		}
		public IActionResult Create()
		{
			ViewBag.Users = _dbContext.Users.ToList(); 
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(TaskItem task)
        {
           

            // Model state is valid, try to find the user and assign them
            Console.WriteLine($"UserId: {task.UserId}");
            var user = await _dbContext.Users.FindAsync(task.UserId);

            if (user != null)
            {
                task.User = user; // Assign the user to the task
                Console.WriteLine($"Task UserId: {task.UserId}, Assigned User: {task.User?.Name}");

                await _dbContext.Tasks.AddAsync(task);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index"); // Redirect to Index after saving the task
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid User"); 
                ViewBag.Users = _dbContext.Users.ToList(); 
                return View(task); 
            }
        }
    }

}
