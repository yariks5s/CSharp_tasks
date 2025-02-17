using Microsoft.AspNetCore.Mvc;
using MvcTasksApp.Models;
using System;
using System.Threading.Tasks;

namespace MvcTasksApp.Controllers
{
    public class TasksController : Controller
    {
        // Завдання 7: Обчислення інтегралу методом трапецій (MVC)
        [HttpGet]
        public IActionResult IntegralMVC()
        {
            return View();
        }

        [HttpPost]
        public IActionResult IntegralMVC(double a, double b, int n)
        {
            // створимо модель інтегратора і виконуємо обчислення
            var integrator = new TrapezoidalIntegrator(a, b, n);
            integrator.Compute(Math.Sin);

            // створимо зображення у форматі Base64
            ViewBag.Integral = integrator.Integral.ToString("F6");
            ViewBag.ChartImage = integrator.GenerateChartBase64();

            return View("IntegralMVCResult");
        }

        // Завдання 8: Бульбашкове сортування (MVC)
        [HttpGet]
        public IActionResult BubbleSortMVC()
        {
            // це сторінка, що містить тег <img> з src="/Tasks/BubbleSortStream"
            return View();
        }

        [HttpGet]
        public async Task BubbleSortStream()
        {
            Response.ContentType = "multipart/x-mixed-replace; boundary=frame";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            // створимо випадковий масив
            Random rnd = new Random();
            int numBars = 30;
            int[] array = new int[numBars];
            for (int i = 0; i < numBars; i++)
            {
                array[i] = rnd.Next(10, 311);
            }

            // створимо модель аніматора сортування
            var animator = new BubbleSortAnimator(array);

            await foreach (var frame in animator.AnimateAsync())
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
