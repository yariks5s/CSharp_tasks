using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Drawing.Processing;
using System;
using System.IO;
using System.Linq;

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

                double h = (b - a) / n;
                Func<double, double> f = x => Math.Sin(x);

                double[] xValues = new double[n + 1];
                double[] yValues = new double[n + 1];

                for (int i = 0; i <= n; i++)
                {
                    double x = a + i * h;
                    xValues[i] = x;
                    yValues[i] = f(x);
                }

                double integral = yValues[0] / 2.0 + yValues[n] / 2.0;
                for (int i = 1; i < n; i++)
                {
                    integral += yValues[i];
                }
                integral *= h;

                string base64Image = GenerateIntegrationChart(a, b, n, xValues, yValues);

                var resultHtml = await File.ReadAllTextAsync("Views/Task5/compute.html");
                resultHtml = resultHtml.Replace("{{Integral}}", integral.ToString("F6"))
                                       .Replace("{{ChartImage}}", base64Image);
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(resultHtml);
            });
        }

        static string GenerateIntegrationChart(double a, double b, int n, double[] xValues, double[] yValues)
        {
            int width = 600, height = 400;
            int margin = 40;
            using (var image = new Image<Rgba32>(width, height))
            {
                image.Mutate(ctx =>
                {
                    ctx.Fill(Color.White);
                    double minY = yValues.Min();
                    double maxY = yValues.Max();
                    double yRange = maxY - minY;
                    minY -= 0.1 * yRange;
                    maxY += 0.1 * yRange;

                    Func<double, float> scaleX = x => margin + (float)((x - a) / (b - a) * (width - 2 * margin));
                    Func<double, float> scaleY = y => margin + (float)((maxY - y) / (maxY - minY) * (height - 2 * margin));

                    // Малюємо осі
                    var axisColor = Color.Black;
                    float axisThickness = 2f;
                    var axisPen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(axisColor, axisThickness);
                    var xAxisPath = new SixLabors.ImageSharp.Drawing.PathBuilder()
                        .AddLine(new PointF(margin, height - margin), new PointF(width - margin, height - margin))
                        .Build();
                    ctx.Draw(axisPen, xAxisPath);
                    var yAxisPath = new SixLabors.ImageSharp.Drawing.PathBuilder()
                        .AddLine(new PointF(margin, margin), new PointF(margin, height - margin))
                        .Build();
                    ctx.Draw(axisPen, yAxisPath);

                    // Малюємо трапеції
                    var trapFill = Color.FromRgba(173, 216, 230, 128);
                    var trapOutline = Color.Blue;
                    for (int i = 0; i < n; i++)
                    {
                        var p0 = new PointF(scaleX(xValues[i]), scaleY(0));
                        var p1 = new PointF(scaleX(xValues[i + 1]), scaleY(0));
                        var p2 = new PointF(scaleX(xValues[i + 1]), scaleY(yValues[i + 1]));
                        var p3 = new PointF(scaleX(xValues[i]), scaleY(yValues[i]));
                        var polygon = new PointF[] { p0, p1, p2, p3 };
                        ctx.FillPolygon(trapFill, polygon);
                        ctx.DrawPolygon(SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(trapOutline, 1f), polygon);
                    }

                    // Малюємо криву функції
                    var curveColor = Color.Red;
                    var curvePen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(curveColor, 2f);
                    var curvePoints = xValues.Select((x, i) => new PointF(scaleX(x), scaleY(yValues[i]))).ToArray();
                    var pathBuilder = new SixLabors.ImageSharp.Drawing.PathBuilder();
                    if (curvePoints.Length > 0)
                    {
                        pathBuilder.StartFigure();
                        for (int i = 1; i < curvePoints.Length; i++)
                            pathBuilder.AddLine(curvePoints[i - 1], curvePoints[i]);
                    }
                    var curvePath = pathBuilder.Build();
                    ctx.Draw(curvePen, curvePath);
                });

                using (var ms = new MemoryStream())
                {
                    image.Save(ms, new PngEncoder());
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
}
