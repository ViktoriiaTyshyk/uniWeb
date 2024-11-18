using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using TyshykWebApp.Models;

namespace TyshykWebApp.Services
{
    public class CalculationManager
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Computation>> _userTasks = new();
        private readonly ILogger<CalculationManager> _logger;
        public CalculationManager(ILogger<CalculationManager> logger) => _logger = logger;

        private string GetUserId(HttpContext httpContext)
        {
            return httpContext.User.Identity?.Name?.ToString() ?? throw new InvalidOperationException("User is not authenticated.");
        }

        public Computation CreateTask(HttpContext httpContext, int rows, int cols)
        {
            var userId = GetUserId(httpContext);

            _logger.LogInformation(userId);

            var task = new Computation
            {
                UserId = userId
            };

            if (!_userTasks.ContainsKey(userId))
            {
                _userTasks[userId] = new ConcurrentDictionary<Guid, Computation>();
            }

            _userTasks[userId][task.Id] = task;
            _logger.LogInformation($"Task added: {task.Id}");

            _ = task.StartCalculationAsync(rows, cols);

            _logger.LogInformation("Task done.");

            return task;
        }

        public Computation? GetTask(HttpContext httpContext, Guid taskId)
        {
            var userId = GetUserId(httpContext);
            return (_userTasks.TryGetValue(userId, out var tasks) && tasks.TryGetValue(taskId, out var task)) ? task : null;
        }

        public void CancelTask(HttpContext httpContext, Guid taskId)
        {
            var userId = GetUserId(httpContext);
            if (_userTasks.TryGetValue(userId, out var tasks) && tasks.TryGetValue(taskId, out var task))
            {
                task.Cancel();
                _logger.LogWarning($"Task canceled: {userId} {taskId}");
            }
        }
    }
}
