using Microsoft.AspNetCore.Mvc;
using System;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Drawing.Processing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;


namespace MvcTasksApp.Controllers
{
    public class TasksController : Controller
    {
        // ----- Завдання 7: Обчислення інтегралу методом трапецій (MVC) -----
        [HttpGet]
        public IActionResult IntegralMVC()
        {
            return View();
        }

        [HttpPost]
        public IActionResult IntegralMVC(double a, double b, int n)
        {
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
                integral += yValues[i];

            integral *= h;

            // робимо зображення діаграми за допомогою ImageSharp
            string base64Image = GenerateIntegrationChart(a, b, n, xValues, yValues);

            ViewBag.Integral = integral.ToString("F6");
            ViewBag.ChartImage = base64Image;

            return View("IntegralMVCResult");
        }

        // ----- Завдання 8: Бульбашкове сортування в реальному часі (MVC) -----
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

            // аідправляємо початковий кадр
            byte[] initialFrame = GenerateBubbleSortChartBytes(array);
            await WriteImageFrame(Response, initialFrame);

            // виконуємо бульбашкове сортування з потоковими оновленнями
            await BubbleSortStreamInternal(array, Response);
        }

        private async Task BubbleSortStreamInternal(int[] array, Microsoft.AspNetCore.Http.HttpResponse response)
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
                        byte[] frame = GenerateBubbleSortChartBytes(array);
                        await WriteImageFrame(response, frame);
                        await Task.Delay(100); // затримка для візуального ефекту
                    }
                }
            }
            // відправляємо фінальний кадр
            byte[] finalFrame = GenerateBubbleSortChartBytes(array);
            await WriteImageFrame(response, finalFrame);
        }

        private async Task WriteImageFrame(Microsoft.AspNetCore.Http.HttpResponse response, byte[] imageBytes)
        {
            string boundary = "--frame\r\n";
            await response.WriteAsync(boundary);
            await response.WriteAsync("Content-Type: image/png\r\n");
            await response.WriteAsync($"Content-Length: {imageBytes.Length}\r\n\r\n");
            await response.Body.WriteAsync(imageBytes, 0, imageBytes.Length);
            await response.WriteAsync("\r\n");
            await response.Body.FlushAsync();
        }


        private string GenerateIntegrationChart(double a, double b, int n, double[] xValues, double[] yValues)
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

                    // Малюємо осі за допомогою PathBuilder
                    var axisColor = Color.Black;
                    float axisThickness = 2f;
                    var axisPen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(axisColor, axisThickness);
                    var xAxis = new SixLabors.ImageSharp.Drawing.PathBuilder()
                        .AddLine(new PointF(margin, height - margin), new PointF(width - margin, height - margin))
                        .Build();
                    ctx.Draw(axisPen, xAxis);
                    var yAxis = new SixLabors.ImageSharp.Drawing.PathBuilder()
                        .AddLine(new PointF(margin, margin), new PointF(margin, height - margin))
                        .Build();
                    ctx.Draw(axisPen, yAxis);

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
                        {
                            pathBuilder.AddLine(curvePoints[i - 1], curvePoints[i]);
                        }
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

        private byte[] GenerateBubbleSortChartBytes(int[] array)
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
