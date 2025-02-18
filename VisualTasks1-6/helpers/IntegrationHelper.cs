using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Drawing.Processing;
using MyProject.Domain;

namespace MyProject.Helpers
{
    public static class IntegrationHelper
    {
        public class IntegrationResult
        {
            public double Integral { get; set; }
            public string Base64Image { get; set; }
        }

        public static IntegrationResult ComputeIntegration(double a, double b, int n, Func<double, double> function)
        {
            var integrator = new TrapezoidalIntegrator(a, b, n);
            integrator.Compute(function);

            string base64Image = GenerateIntegrationChart(a, b, n, integrator.XValues, integrator.YValues);
            return new IntegrationResult
            {
                Integral = integrator.Integral,
                Base64Image = base64Image
            };
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
