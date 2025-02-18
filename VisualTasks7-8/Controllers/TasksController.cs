using Microsoft.AspNetCore.Mvc;
using MvcTasksApp.Services;
using MvcTasksApp.Helpers;
using System.Threading.Tasks;

namespace MvcTasksApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly IIntegrationService _integrationService;
        private readonly IBubbleSortService _bubbleSortService;

        public TasksController(IIntegrationService integrationService, IBubbleSortService bubbleSortService)
        {
            _integrationService = integrationService;
            _bubbleSortService = bubbleSortService;
        }

        // Завдання 7: Обчислення інтегралу методом трапецій (MVC)
        [HttpGet]
        public IActionResult IntegralMVC()
        {
            return View();
        }

        [HttpPost]
        public IActionResult IntegralMVC(double a, double b, int n)
        {
            var result = IntegrationHelper.ComputeIntegration(_integrationService, a, b, n);
            ViewBag.Integral = result.Integral.ToString("F6");
            ViewBag.ChartImage = result.ChartBase64;
            return View("IntegralMVCResult");
        }

        // Завдання 8: Бульбашкове сортування (MVC)
        [HttpGet]
        public IActionResult BubbleSortMVC()
        {
            return View();
        }

        [HttpGet]
        public async Task BubbleSortStream()
        {
            await SortingHelper.StreamBubbleSort(_bubbleSortService, Response);
        }
    }
}
