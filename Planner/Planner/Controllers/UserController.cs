using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Planner.Data;
using Planner.Helpers;
using Planner.Models;

[UserAuthorize]
public class UserTaskController : Controller
{
    private readonly PlannerDbContext _dbContext;

    public UserTaskController(PlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IActionResult> Index()
    {
        if (Request.Cookies.TryGetValue("session_id", out string sessionToken))
        {
            var session = await _dbContext.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.ExpirationTime > DateTime.UtcNow);

            if (session != null)
            {
                var userId = session.UserId;

                var tasks = await _dbContext.Tasks
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                return View(tasks);
            }
        }

        return Unauthorized(); 
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsDone(int id)
    {
        if (Request.Cookies.TryGetValue("session_id", out string sessionToken))
        {
            var session = await _dbContext.Sessions
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.ExpirationTime > DateTime.UtcNow);

            if (session != null)
            {
                var userId = session.UserId;

                var task = await _dbContext.Tasks
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (task == null)
                {
                    return NotFound("Task not found or you don't have permission.");
                }

                if (task.Deadline < DateTime.UtcNow)
                {
                    return BadRequest("Cannot mark task as completed after the deadline.");
                }

                task.IsCompleted = true;
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
        }

        return Unauthorized(); 
    }

}
