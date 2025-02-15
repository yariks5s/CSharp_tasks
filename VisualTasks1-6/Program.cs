using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Drawing.Processing; // для розширень малювання
using System.IO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

#region меню завдань 1–6
app.MapGet("/", async context =>
{
    await context.Response.WriteAsync(@"
<html>
  <head>
    <meta charset='utf-8'/>
    <title>Меню завдань (1–6)</title>
    <style>
      body { font-family: Arial; text-align: center; margin-top: 50px; }
      .tile {
         display: inline-block;
         width: 400px;
         padding: 15px;
         margin: 10px;

         background: #f0f0f0;
         border: 1px solid #ccc;
         border-radius: 8px;
         text-decoration: none;
         color: #333;

         font-size: 16px;
      }
      .tile:hover { background: #e0e0e0; }
    </style>
  </head>
  <body>
    <h1>Меню завдань (1–6)</h1>
  
    <a class='tile' href='/task1'>1. Quicksort (Thread)</a><br>
    <a class='tile' href='/task2'>2. Quicksort (Tasks)</a><br>
    <a class='tile' href='/task3'>3. Quicksort (TaskFactory)</a><br>
    <a class='tile' href='/task4'>4. Quicksort (Parallel)</a><br>

    <a class='tile' href='/task5'>5. Інтеграл методом трапецій з візуалізацією</a><br>
    <a class='tile' href='/task6'>6. Бульбашкове сортування з візуалізацією</a>
  </body>
</html>
");
});
#endregion

#region Завдання 1 – Quicksort (Thread)
app.MapGet("/task1", async context =>
{
    await context.Response.WriteAsync(@"
<html>
<head>
  <meta charset='utf-8'/>
  <title>Quicksort (Thread)</title>
</head>
<body>
  <h1>Quicksort із використанням Thread</h1>

  <form method='post' action='/task1/sort'>

    <label>Введіть числа (через кому):</label><br>
    <input type='text' name='numbers' size='50'/><br><br>
    <input type='submit' value='Сортувати'/>

  </form>

  <br><a href='/'>Меню</a>
</body>
</html>
");
});

app.MapPost("/task1/sort", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var numbersStr = form["numbers"].ToString();
    int[] numbers = numbersStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => int.Parse(s.Trim()))
                              .ToArray();

    QuickSortThread(numbers, 0, numbers.Length - 1);

    await context.Response.WriteAsync($@"
<html>
<head><meta charset='utf-8'/><title>Результат (Thread)</title></head>
<body>

  <h1>Відсортований масив (Thread)</h1>
  <p>{string.Join(", ", numbers)}</p>
  <a href='/task1'>Назад</a> | <a href='/'>Меню</a>

</body>
</html>
");
});

static void QuickSortThread(int[] arr, int left, int right)
{
    if (left < right)
    {
        int pivot = Partition(arr, left, right);
        int threshold = 1000;
        if (right - left > threshold)
        {

            Thread leftThread = new Thread(() => QuickSortThread(arr, left, pivot - 1));
            Thread rightThread = new Thread(() => QuickSortThread(arr, pivot + 1, right));
            leftThread.Start();
            rightThread.Start();
            leftThread.Join();
            rightThread.Join();

        }
        else
        {

            QuickSortThread(arr, left, pivot - 1);
            QuickSortThread(arr, pivot + 1, right);

        }
    }
}
#endregion

#region Завдання 2 – Quicksort (Tasks)
app.MapGet("/task2", async context =>
{
    await context.Response.WriteAsync(@"
<html>
<head>
  <meta charset='utf-8'/>
  <title>Quicksort (Tasks)</title>
</head>
<body>
  <h1>Quicksort із використанням Tasks</h1>
  <form method='post' action='/task2/sort'>
    <label>Введіть числа (через кому):</label><br>
    <input type='text' name='numbers' size='50'/><br><br>
    <input type='submit' value='Сортувати'/>
  </form>
  <br><a href='/'>Меню</a>
</body>
</html>
");
});

app.MapPost("/task2/sort", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var numbersStr = form["numbers"].ToString();
    int[] numbers = numbersStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => int.Parse(s.Trim()))
                              .ToArray();

    await QuickSortTasks(numbers, 0, numbers.Length - 1);

    await context.Response.WriteAsync($@"
<html>
<head><meta charset='utf-8'/><title>Результат (Tasks)</title></head>
<body>
  <h1>Відсортований масив (Tasks)</h1>
  <p>{string.Join(", ", numbers)}</p>
  <a href='/task2'>Назад</a> | <a href='/'>Меню</a>
</body>
</html>
");
});

static async Task QuickSortTasks(int[] arr, int left, int right)
{
    if (left < right)
    {
        int pivot = Partition(arr, left, right);
        int threshold = 1000;
        if (right - left > threshold)
        {
            Task leftTask = Task.Run(() => QuickSortTasks(arr, left, pivot - 1));
            Task rightTask = Task.Run(() => QuickSortTasks(arr, pivot + 1, right));
            await Task.WhenAll(leftTask, rightTask);
        }
        else
        {
            await QuickSortTasks(arr, left, pivot - 1);
            await QuickSortTasks(arr, pivot + 1, right);
        }
    }
}
#endregion

#region Завдання 3 – Quicksort (TaskFactory)
app.MapGet("/task3", async context =>
{
    await context.Response.WriteAsync(@"
<html>
<head>
  <meta charset='utf-8'/>
  <title>Quicksort (TaskFactory)</title>
</head>
<body>
  <h1>Quicksort із використанням TaskFactory</h1>
  <form method='post' action='/task3/sort'>
    <label>Введіть числа (через кому):</label><br>
    <input type='text' name='numbers' size='50'/><br><br>
    <input type='submit' value='Сортувати'/>
  </form>
  <br><a href='/'>Меню</a>
</body>
</html>
");
});

app.MapPost("/task3/sort", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var numbersStr = form["numbers"].ToString();

    int[] numbers = numbersStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => int.Parse(s.Trim()))
                              .ToArray();

    TaskFactory factory = new TaskFactory();
    await QuickSortTaskFactory(numbers, 0, numbers.Length - 1, factory);
    await context.Response.WriteAsync($@"
<html>
<head><meta charset='utf-8'/><title>Результат (TaskFactory)</title></head>
<body>

  <h1>Відсортований масив (TaskFactory)</h1>
  <p>{string.Join(", ", numbers)}</p>
  <a href='/task3'>Назад</a> | <a href='/'>Меню</a>

</body>
</html>
");
});

static Task QuickSortTaskFactory(int[] arr, int left, int right, TaskFactory factory)
{
    if (left < right)
    {
        int pivot = Partition(arr, left, right);

        int threshold = 1000;

        if (right - left > threshold)
        {
            Task leftTask = factory.StartNew(() => QuickSortTaskFactory(arr, left, pivot - 1, factory)).Unwrap();
            Task rightTask = factory.StartNew(() => QuickSortTaskFactory(arr, pivot + 1, right, factory)).Unwrap();
            return Task.WhenAll(leftTask, rightTask);
        }
        else
        {
            QuickSortTaskFactory(arr, left, pivot - 1, factory).Wait();
            QuickSortTaskFactory(arr, pivot + 1, right, factory).Wait();
        }

    }
    return Task.CompletedTask;
}
#endregion

#region Завдання 4 – Quicksort (Parallel)
app.MapGet("/task4", async context =>
{
    await context.Response.WriteAsync(@"
<html>
<head>
  <meta charset='utf-8'/>
  <title>Quicksort (Parallel)</title>
</head>
<body>
  <h1>Quicksort із використанням Parallel</h1>

  <form method='post' action='/task4/sort'>
    <label>Введіть числа (через кому):</label><br>
    <input type='text' name='numbers' size='50'/><br><br>
    <input type='submit' value='Сортувати'/>
  </form>

  <br><a href='/'>Меню</a>
</body>
</html>
");
});

app.MapPost("/task4/sort", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var numbersStr = form["numbers"].ToString();


    int[] numbers = numbersStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => int.Parse(s.Trim()))
                              .ToArray();
    QuickSortParallel(numbers, 0, numbers.Length - 1);
    await context.Response.WriteAsync($@"
<html>
<head><meta charset='utf-8'/><title>Результат (Parallel)</title></head>
<body>

  <h1>Відсортований масив (Parallel)</h1>
  <p>{string.Join(", ", numbers)}</p>
  <a href='/task4'>Назад</a> | <a href='/'>Меню</a>

</body>
</html>
");
});

static void QuickSortParallel(int[] arr, int left, int right)
{
    if (left < right)
    {
        int pivot = Partition(arr, left, right);
        int threshold = 1000;

        if (right - left > threshold)
        {
            System.Threading.Tasks.Parallel.Invoke(
                () => QuickSortParallel(arr, left, pivot - 1),
                () => QuickSortParallel(arr, pivot + 1, right)
            );
        }
        else
        {
            QuickSortParallel(arr, left, pivot - 1);
            QuickSortParallel(arr, pivot + 1, right);
        }
    }
}

static int Partition(int[] arr, int left, int right)
{
    int pivot = arr[right];
    int i = left - 1;

    for (int j = left; j < right; j++)
    {
        if (arr[j] <= pivot)
        {
            i++;
            Swap(arr, i, j);
        }
    }
    Swap(arr, i + 1, right);
    return i + 1;
}

static void Swap(int[] arr, int i, int j)
{
    int temp = arr[i];
    arr[i] = arr[j];
    arr[j] = temp;
}
#endregion

#region Завдання 5 – Інтеграл методом трапецій з візуалізацією (ImageSharp)
app.MapGet("/task5", async context =>
{
    await context.Response.WriteAsync(@"
<html>
<head>
  <meta charset='utf-8'/>
  <title>Інтеграл методом трапецій</title>
</head>
<body>

  <h1>Обчислення інтегралу методом трапецій</h1>

  <form method='post' action='/task5/compute'>
    <label>Нижня межа (a):</label>
    <input type='text' name='a' value='0'/><br><br>
    <label>Верхня межа (b):</label>
    <input type='text' name='b' value='3.1416'/><br><br>
    <label>Кількість підінтервалів (n):</label>
    <input type='text' name='n' value='10'/><br><br>
    <input type='submit' value='Обчислити'/>
  </form>

  <br><a href='/'>Меню</a>

</body>
</html>
");
});

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

    // тут робимо зображення діаграми за допомогою ImageSharp
    string base64Image = GenerateIntegrationChart(a, b, n, xValues, yValues);

    await context.Response.WriteAsync($@"
<html>
<head>
  <meta charset='utf-8'/>
  <title>Результат інтегрування</title>
</head>
<body>
  <h1>Результат інтегрування методом трапецій</h1>
  <p>Обчислений інтеграл: {integral:F6}</p>
  <h2>Візуалізація:</h2>
  <img src='data:image/png;base64,{base64Image}' alt='Integration Chart'/><br>
  <a href='/task5'>Назад</a> | <a href='/'>Меню</a>
</body>
</html>
");
});

// Функція генерації зображення інтегрування (ImageSharp)
string GenerateIntegrationChart(double a, double b, int n, double[] xValues, double[] yValues)
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

            // робимо осі через побудову шляху (PathBuilder)
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

            // робимо трапеції
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

            // роибмо криву функції
            var curveColor = Color.Red;
            var curvePen = SixLabors.ImageSharp.Drawing.Processing.Pens.Solid(curveColor, 2f);
            var curvePoints = xValues.Select((x, i) => new PointF(scaleX(x), scaleY(yValues[i]))).ToArray();
            // побудова лінійного шляху для кривої
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
#endregion

#region Завдання 6 – Бульбашкове сортування з візуалізацією в реальному часі (ImageSharp Streaming)
app.MapGet("/task6", async context =>
{
    // HTML-сторінка, яка містить <img> для потокового відображення сортування
    await context.Response.WriteAsync(@"
<html>
<head>
  <meta charset='utf-8'/>
  <title>Бульбашкове сортування в реальному часі</title>
  <style>
    body { text-align: center; font-family: Arial; }
  </style>
</head>
<body>

  <h1>Бульбашкове сортування в реальному часі</h1>
  <img src='/task6/stream' alt='Sorting...' />
  <br><a href='/task6'>Скинути сортування</a> | <a href='/'>Меню</a>

</body>
</html>
");
});

// потоковий ендпоінт для реального відображення сортування
app.MapGet("/task6/stream", async context =>
{
    context.Response.ContentType = "multipart/x-mixed-replace; boundary=frame";
    context.Response.Headers["Cache-Control"] = "no-cache";
    context.Response.Headers["Connection"] = "keep-alive";

    // генеруємо початковий випадковий масив
    Random rnd = new Random();
    int numBars = 30;
    int[] array = new int[numBars];
    for (int i = 0; i < numBars; i++)
        array[i] = rnd.Next(10, 311);

    // відправляємо початковий кадр
    byte[] initialBytes = GenerateBubbleSortChartBytes(array);
    await WriteImageFrame(context.Response, initialBytes);

    // запускаємо сортування з потоковими оновленнями
    await BubbleSortStream(array, context.Response);
});

async Task BubbleSortStream(int[] array, HttpResponse response)
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
                // після кожного swap генеруємо та відправляємо новий кадр
                byte[] imageBytes = GenerateBubbleSortChartBytes(array);
                await WriteImageFrame(response, imageBytes);
                await Task.Delay(100); // затримка для візуального ефекту
            }
        }
    }
    // відправляємо фінальний кадр
    byte[] finalBytes = GenerateBubbleSortChartBytes(array);
    await WriteImageFrame(response, finalBytes);
}

async Task WriteImageFrame(HttpResponse response, byte[] imageBytes)
{
    string boundary = "--frame\r\n";
    await response.WriteAsync(boundary);
    await response.WriteAsync("Content-Type: image/png\r\n");
    await response.WriteAsync($"Content-Length: {imageBytes.Length}\r\n\r\n");
    await response.Body.WriteAsync(imageBytes, 0, imageBytes.Length);
    await response.WriteAsync("\r\n");
    await response.Body.FlushAsync();
}

byte[] GenerateBubbleSortChartBytes(int[] array)
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
#endregion

app.Run();
