using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Planner.Data;
using Planner.Helpers;
using Planner.Models;

public class UserTaskController : Controller
{
    private readonly PlannerDbContext _dbContext;
    [UserAuthorize]
    public UserTaskController(PlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IActionResult> Index()
    {
        var userIdString = HttpContext.Request.Headers["UserId"];

        if (int.TryParse(userIdString, out int userId))
        {
            var tasks = await _dbContext.Tasks
       .Where(t => t.UserId == userId)
       .ToListAsync();
            return View(tasks);
        }
        else
        {
          
            return BadRequest("Invalid UserId");
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsDone(int id)
    {
        var task = await _dbContext.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();

        var userIdCookie = Request.Cookies["user_id"];
        if (string.IsNullOrEmpty(userIdCookie) || !int.TryParse(userIdCookie, out var userId) || task.UserId != userId)
            return Unauthorized();

        if (DateTime.Now <= task.Deadline)
        {
            task.IsCompleted = true;
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}
