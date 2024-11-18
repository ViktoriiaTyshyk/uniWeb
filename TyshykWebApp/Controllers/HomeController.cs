using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TyshykWebApp.Data;
using TyshykWebApp.Models;
using TyshykWebApp.Services;

namespace TyshykWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly CalculationManager _calculationManager;
        private readonly ILogger<HomeController> _logger;
        private readonly ComputationDBContext _computationDBContext;
        
        public HomeController(CalculationManager calculationManager, ComputationDBContext computationDBContext, ILogger<HomeController> logger)
        {
            _calculationManager = calculationManager;
            _computationDBContext = computationDBContext;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ProcessMatrices(int rows, int cols)
        {
            _logger.LogInformation("Start processing");
            if (rows <= 0 || cols <= 0)
            {
                ModelState.AddModelError("", "Rows and columns must be greater then 0");
                return View("Index");
            }

            var task = _calculationManager.CreateTask(HttpContext, rows, cols);

            Task.Delay(100);

            return RedirectToAction("TaskStatus", new { id = task.Id });
        }

        [HttpGet]
        public IActionResult TaskStatus(Guid id)
        {
            var task = _calculationManager.GetTask(HttpContext, id);
            if (task == null) 
            {
                NotFound();
            }
            _computationDBContext.AddAsync(task);
            _computationDBContext.SaveChangesAsync();
            return View(task);
        }

        [HttpPost]
        public IActionResult CancelTask(Guid id)
        {
            _calculationManager.CancelTask(HttpContext, id);
            return RedirectToAction("TaskStatus", new { id });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> TaskHistory()
        {
            var userId = HttpContext.User.Identity?.Name?.ToString();
            if (userId != null)
            {
                var taskHistory = await _computationDBContext.ComputationSet.OrderByDescending(task => task.StartTime).Where(t => t.UserId == userId).ToListAsync();
                return View(taskHistory);
            }
            return NotFound();
        }
    }
}
