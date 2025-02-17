using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace MvcTasksApp.Models
{
    public class TrapezoidalIntegrator
    {
        public double A { get; }
        public double B { get; }
        public int N { get; }
        public double Integral { get; private set; }
        public double[] XValues { get; private set; }
        public double[] YValues { get; private set; }

        public TrapezoidalIntegrator(double a, double b, int n)
        {
            A = a;
            B = b;
            N = n;
        }

        public void Compute(Func<double, double> function)
        {
            double h = (B - A) / N;
            XValues = new double[N + 1];
            YValues = new double[N + 1];

            for (int i = 0; i <= N; i++)
            {
                double x = A + i * h;
                XValues[i] = x;
                YValues[i] = function(x);
            }

            Integral = YValues[0] / 2.0 + YValues[N] / 2.0;
            for (int i = 1; i < N; i++)
                Integral += YValues[i];
            Integral *= h;
        }

        public string GenerateChartBase64()
        {
            int width = 600, height = 400;
            int margin = 40;

            using (var image = new Image<Rgba32>(width, height))
            {
                image.Mutate(ctx =>
                {
                    ctx.Fill(Color.White);

                    double minY = YValues.Min();
                    double maxY = YValues.Max();
                    double yRange = maxY - minY;
                    minY -= 0.1 * yRange;
                    maxY += 0.1 * yRange;

                    Func<double, float> scaleX = x => margin + (float)((x - A) / (B - A) * (width - 2 * margin));
                    Func<double, float> scaleY = y => margin + (float)((maxY - y) / (maxY - minY) * (height - 2 * margin));

                    // малюємо осі
                    var axisPen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(Color.Black, 2f);
                    var xAxis = new SixLabors.ImageSharp.Drawing.PathBuilder()
                        .AddLine(new PointF(margin, height - margin), new PointF(width - margin, height - margin))
                        .Build();
                    ctx.Draw(axisPen, xAxis);

                    var yAxis = new SixLabors.ImageSharp.Drawing.PathBuilder()
                        .AddLine(new PointF(margin, margin), new PointF(margin, height - margin))
                        .Build();
                    ctx.Draw(axisPen, yAxis);

                    // млюємо трапеції
                    var trapFill = Color.FromRgba(173, 216, 230, 128);
                    var trapOutline = Color.Blue;
                    for (int i = 0; i < N; i++)
                    {
                        var p0 = new PointF(scaleX(XValues[i]), scaleY(0));
                        var p1 = new PointF(scaleX(XValues[i + 1]), scaleY(0));
                        var p2 = new PointF(scaleX(XValues[i + 1]), scaleY(YValues[i + 1]));
                        var p3 = new PointF(scaleX(XValues[i]), scaleY(YValues[i]));
                        var polygon = new PointF[] { p0, p1, p2, p3 };
                        ctx.FillPolygon(trapFill, polygon);
                        ctx.DrawPolygon(SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(trapOutline, 1f), polygon);
                    }

                    // малюємо функцію
                    var curvePen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(Color.Red, 2f);
                    var curvePoints = XValues.Select((x, i) => new PointF(scaleX(x), scaleY(YValues[i]))).ToArray();
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
