using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using pr8_trpo.Models;

namespace pr8_trpo.Controllers
{
    [ApiController]
    [Route("api/v1/tasks")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly TasksDbContext _context;

        public TaskController(TasksDbContext context)
        {
            _context = context;
        }

        public record CreateTaskRequest(string Title, string Description);
        public record UpdateTaskRequest(string Title, string Description, bool IsCompleted);

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult GetTasks()
        {
            var login = User.Identity.Name;

            var tasks = _context.Tasks
                .Where(t => t.OwnerLogin == login)
                .ToList();

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult GetTask(int id)
        {
            var task = _context.Tasks.Find(id);

            if (task == null)
                return NotFound();

            if (!User.IsInRole("Admin") && task.OwnerLogin != User.Identity.Name)
                return Forbid();

            return Ok(task);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult CreateTask(CreateTaskRequest request)
        {
            var task = new Models.Task
            {
                Title = request.Title,
                Description = request.Description,
                OwnerLogin = User.Identity.Name,
                IsCompleted = false
            };

            _context.Tasks.Add(task);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult UpdateTask(int id, UpdateTaskRequest request)
        {
            var task = _context.Tasks.Find(id);

            if (task == null)
                return NotFound();

            if (!User.IsInRole("Admin") && task.OwnerLogin != User.Identity.Name)
                return Forbid();

            task.Title = request.Title;
            task.Description = request.Description;
            task.IsCompleted = request.IsCompleted;

            _context.SaveChanges();
            return Ok(task);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult DeleteTask(int id)
        {
            var task = _context.Tasks.Find(id);

            if (task == null)
                return NotFound();

            if (!User.IsInRole("Admin") && task.OwnerLogin != User.Identity.Name)
                return Forbid();

            _context.Tasks.Remove(task);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllTasks()
        {
            var tasks = _context.Tasks.ToList();

            return Ok(tasks);
        }
    }
}
