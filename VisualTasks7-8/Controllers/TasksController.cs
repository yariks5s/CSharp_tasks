using Microsoft.AspNetCore.Mvc;
using MvcTasksApp.Services;
using System;
using System.Threading.Tasks;

namespace MvcTasksApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly IIntegrationService _integrationService;
        private readonly IBubbleSortService _bubbleSortService;

        // сервіси інжектуються через конструктор
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
            var result = _integrationService.ComputeIntegration(a, b, n);
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
            Response.ContentType = "multipart/x-mixed-replace; boundary=frame";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            // Генерация случайного массива
            Random rnd = new Random();
            int numBars = 30;
            int[] array = new int[numBars];
            for (int i = 0; i < numBars; i++)
                array[i] = rnd.Next(10, 311);

            await foreach (var frame in _bubbleSortService.GetBubbleSortAnimationAsync(array))
            {
                await WriteImageFrame(frame);
            }
        }

        private async Task WriteImageFrame(byte[] imageBytes)
        {
            string boundary = "--frame\r\n";
            await Response.WriteAsync(boundary);
            await Response.WriteAsync("Content-Type: image/png\r\n");
            await Response.WriteAsync($"Content-Length: {imageBytes.Length}\r\n\r\n");
            await Response.Body.WriteAsync(imageBytes, 0, imageBytes.Length);
            await Response.WriteAsync("\r\n");
            await Response.Body.FlushAsync();
        }
    }
}
