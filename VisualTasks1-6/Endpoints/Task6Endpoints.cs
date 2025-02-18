using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MyProject.Helpers;
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
                await SortingHelper.StreamSortingFrames(context);
            });
        }
    }
}
