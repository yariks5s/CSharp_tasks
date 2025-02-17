using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;

namespace MyProject.Domain
{
    public class BubbleSortAnimator
    {
        private int[] array;
        public int Delay { get; set; } = 100; // затримка в мілісекундах між кадрами
        public int Width { get; set; } = 600;
        public int Height { get; set; } = 400;
        public int Margin { get; set; } = 20;

        public BubbleSortAnimator(int[] initialArray)
        {
            // беремо копію масиву, щоб не змінювати оригінал
            array = (int[])initialArray.Clone();
        }

        public async IAsyncEnumerable<byte[]> AnimateAsync()
        {
            // Надсилаємо початковий кадр
            yield return GenerateChartBytes(array);

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

                        yield return GenerateChartBytes(array);
                        await Task.Delay(Delay);
                    }
                }
            }
            // Надсилаємо фінальний кадр
            yield return GenerateChartBytes(array);
        }

        byte[] GenerateChartBytes(int[] currentArray)
        {
            int n = currentArray.Length;
            int barWidth = (Width - 2 * Margin) / n;
            int maxVal = currentArray.Max();

            using (var image = new Image<Rgba32>(Width, Height))
            {
                image.Mutate(ctx =>
                {
                    ctx.Fill(Color.White);
                    for (int i = 0; i < n; i++)
                    {
                        int barHeight = (int)((currentArray[i] / (double)maxVal) * (Height - 2 * Margin));
                        int x = Margin + i * barWidth;
                        int y = Height - Margin - barHeight;
                        var rect = new SixLabors.ImageSharp.Rectangle(x, y, barWidth - 2, barHeight);
                        ctx.Fill(Color.SteelBlue, rect);
                        ctx.Draw(Pens.Solid(Color.Black, 1f), rect);
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
