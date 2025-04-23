using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Planner.Data;
using Planner.Helpers;
using Planner.Models;

namespace Planner.Controllers
{
    [AdminAuthorize]
    public class AdminTaskController : Controller
	{
		private readonly PlannerDbContext _dbContext;
		public AdminTaskController(PlannerDbContext dbContext)
		{
			_dbContext = dbContext;
		}
        [AdminAuthorize]
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
           

            
            
            var user = await _dbContext.Users.FindAsync(task.UserId);

            if (user != null)
            {
                task.User = user; 
                

                await _dbContext.Tasks.AddAsync(task);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Index"); 
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid User"); 
                ViewBag.Users = _dbContext.Users.ToList(); 
                return View(task); 
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var task = await _dbContext.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            ViewBag.Users = _dbContext.Users.ToList();
            return View(task);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(TaskItem task)
        {
           
            
            _dbContext.Tasks.Update(task);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
            
            ViewBag.Users = _dbContext.Users.ToList();
            return View(task);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _dbContext.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            _dbContext.Tasks.Remove(task);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }

}
