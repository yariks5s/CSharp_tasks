using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using MyProject.Endpoints; // Простір імен із нашими ендпоінтами

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

// Головне меню – читаємо HTML із файлу Views/index.html
app.MapGet("/", async context =>
{
    var html = await System.IO.File.ReadAllTextAsync("Views/index.html");
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(html);
});

// Реєструємо ендпоінти для завдань
app.MapTask1Endpoints();
app.MapTask5Endpoints();
app.MapTask6Endpoints();

app.Run();
