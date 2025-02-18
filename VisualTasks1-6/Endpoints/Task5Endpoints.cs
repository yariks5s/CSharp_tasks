using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MyProject.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyProject.Endpoints
{
    public static class Task5Endpoints
    {
        public static void MapTask5Endpoints(this IEndpointRouteBuilder app)
        {
            // GET – форма для інтегрування
            app.MapGet("/task5", async context =>
            {
                var html = await File.ReadAllTextAsync("Views/Task5/index.html");
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
            });

            // POST – обчислення інтегралу та відображення результату
            app.MapPost("/task5/compute", async context =>
            {
                var form = await context.Request.ReadFormAsync();
                double a = double.Parse(form["a"]);
                double b = double.Parse(form["b"]);
                int n = int.Parse(form["n"]);

                // викликаємо хелпер для обчислення інтегралу та побудови графіка
                var result = IntegrationHelper.ComputeIntegration(a, b, n, Math.Sin);

                var resultHtml = await File.ReadAllTextAsync("Views/Task5/compute.html");
                resultHtml = resultHtml.Replace("{{Integral}}", result.Integral.ToString("F6"))
                                       .Replace("{{ChartImage}}", result.Base64Image);
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(resultHtml);
            });
        }
    }
}
