using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MyProject.Domain;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyProject.Endpoints
{
    public static class Task6Endpoints
    {
        public static void MapTask6Endpoints(this IEndpointRouteBuilder app)
        {
            // GET – сторінка з відображенням потокового сортування
            app.MapGet("/task6", async context =>
            {
                var html = await File.ReadAllTextAsync("Views/Task6/index.html");
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
            });

            // GET – ендпоінт для потокової передачі кадрів сортування
            app.MapGet("/task6/stream", async context =>
            {
                context.Response.ContentType = "multipart/x-mixed-replace; boundary=frame";
                context.Response.Headers["Cache-Control"] = "no-cache";
                context.Response.Headers["Connection"] = "keep-alive";

                // Генеруємо початковий випадковий масив
                Random rnd = new Random();
                int numBars = 30;
                int[] array = new int[numBars];
                for (int i = 0; i < numBars; i++)
                    array[i] = rnd.Next(10, 311);

                // створюєм доменну модель для анімації сортування
                var animator = new BubbleSortAnimator(array);

                await foreach (var frame in animator.AnimateAsync())
                {
                    await WriteImageFrame(context, frame);
                }
            });
        }

        static async Task WriteImageFrame(HttpContext context, byte[] imageBytes)
        {
            string boundary = "--frame\r\n";
            await context.Response.WriteAsync(boundary);
            await context.Response.WriteAsync("Content-Type: image/png\r\n");
            await context.Response.WriteAsync($"Content-Length: {imageBytes.Length}\r\n\r\n");
            await context.Response.Body.WriteAsync(imageBytes, 0, imageBytes.Length);
            await context.Response.WriteAsync("\r\n");
            await context.Response.Body.FlushAsync();
        }
    }
}
