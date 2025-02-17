using MvcTasksApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// реєструємо  сервіси
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<IBubbleSortService, BubbleSortService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
