using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Drawing.Processing;

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

                // Відправляємо початковий кадр
                byte[] initialBytes = GenerateBubbleSortChartBytes(array);
                await WriteImageFrame(context, initialBytes);

                // Запускаємо сортування з потоковими оновленнями
                await BubbleSortStream(context, array);
            });
        }

        static async Task BubbleSortStream(HttpContext context, int[] array)
        {
            int n = array.Length;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (array[j] > array[j + 1])
                    {
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;

                        byte[] imageBytes = GenerateBubbleSortChartBytes(array);
                        await WriteImageFrame(context, imageBytes);
                        await Task.Delay(100);
                    }
                }
            }
            byte[] finalBytes = GenerateBubbleSortChartBytes(array);
            await WriteImageFrame(context, finalBytes);
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

        static byte[] GenerateBubbleSortChartBytes(int[] array)
        {
            int width = 600, height = 400;
            int margin = 20;
            int n = array.Length;
            int barWidth = (width - 2 * margin) / n;
            int maxVal = array.Max();

            using (var image = new Image<Rgba32>(width, height))
            {
                image.Mutate(ctx =>
                {
                    ctx.Fill(Color.White);
                    for (int i = 0; i < n; i++)
                    {
                        int barHeight = (int)((array[i] / (double)maxVal) * (height - 2 * margin));
                        int x = margin + i * barWidth;
                        int y = height - margin - barHeight;
                        var rect = new SixLabors.ImageSharp.Rectangle(x, y, barWidth - 2, barHeight);
                        ctx.Fill(Color.SteelBlue, rect);
                        ctx.Draw(SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(Color.Black, 1f), rect);
                    }
                });
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, new PngEncoder());
                    return ms.ToArray();
                }
            }
        }
    }
}
