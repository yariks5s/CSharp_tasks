using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyProject.Endpoints
{
    public static class Task1Endpoints
    {
        // добавлено: константа для максимального числа потоків та SemaphoreSlim для їх обмеження
        private const int MAX_THREADS = 4;
        private static SemaphoreSlim threadLimiter = new SemaphoreSlim(MAX_THREADS);

        public static void MapTask1Endpoints(this IEndpointRouteBuilder app)
        {
            // GET – форма введення для завдання 1
            app.MapGet("/task1", async context =>
            {
                var html = await System.IO.File.ReadAllTextAsync("Views/Task1/index.html");
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
            });

            // POST – сортування та відображення результатів для всіх 4 варіантів
            app.MapPost("/task1/sort", async context =>
            {
                var form = await context.Request.ReadFormAsync();
                // Використовуємо null-forgiving (!) для уникнення попереджень – або додайте перевірку
                string numbersStr = form["numbers"].ToString()!;
                int[] unsorted = numbersStr
                                    .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                    .Select(s => int.Parse(s.Trim()))
                                    .ToArray();

                // Створюємо копії масиву для кожного методу
                int[] arrayThread = (int[])unsorted.Clone();
                int[] arrayTasks = (int[])unsorted.Clone();
                int[] arrayTaskFactory = (int[])unsorted.Clone();
                int[] arrayParallel = (int[])unsorted.Clone();

                // Вимірюємо час для кожного методу

                // 1. Варіант: Thread
                Stopwatch swThread = Stopwatch.StartNew();
                QuickSortThread(arrayThread, 0, arrayThread.Length - 1);
                swThread.Stop();

                // 2. Варіант: Tasks
                Stopwatch swTasks = Stopwatch.StartNew();
                await QuickSortTasks(arrayTasks, 0, arrayTasks.Length - 1);
                swTasks.Stop();

                // 3. Варіант: TaskFactory
                Stopwatch swTaskFactory = Stopwatch.StartNew();
                TaskFactory factory = new TaskFactory();
                await QuickSortTaskFactory(arrayTaskFactory, 0, arrayTaskFactory.Length - 1, factory);
                swTaskFactory.Stop();

                // 4. Варіант: Parallel.Invoke
                Stopwatch swParallel = Stopwatch.StartNew();
                QuickSortParallel(arrayParallel, 0, arrayParallel.Length - 1);
                swParallel.Stop();

                // Читаємо HTML-шаблон для відображення результатів
                var resultHtml = await System.IO.File.ReadAllTextAsync("Views/Task1/sort.html");
                resultHtml = resultHtml.Replace("{{ThreadResult}}", string.Join(", ", arrayThread))
                                       .Replace("{{ThreadTime}}", swThread.ElapsedMilliseconds.ToString())
                                       .Replace("{{TasksResult}}", string.Join(", ", arrayTasks))
                                       .Replace("{{TasksTime}}", swTasks.ElapsedMilliseconds.ToString())
                                       .Replace("{{TaskFactoryResult}}", string.Join(", ", arrayTaskFactory))
                                       .Replace("{{TaskFactoryTime}}", swTaskFactory.ElapsedMilliseconds.ToString())
                                       .Replace("{{ParallelResult}}", string.Join(", ", arrayParallel))
                                       .Replace("{{ParallelTime}}", swParallel.ElapsedMilliseconds.ToString());

                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(resultHtml);
            });
        }

        // Варіант 1: Quicksort із використанням Thread
        static void QuickSortThread(int[] arr, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(arr, left, right);
                int threshold = 1000;
                if (right - left > threshold)
                {
                    Thread leftThread = null;
                    Thread rightThread = null;

                    // пробуємо отримати дозвіл для створення нового потоку для лівої частини
                    if (threadLimiter.Wait(0))
                    {
                        leftThread = new Thread(() =>
                        {
                            try
                            {
                                QuickSortThread(arr, left, pivot - 1);
                            }
                            finally
                            {
                                threadLimiter.Release();
                            }
                        });
                        leftThread.Start();
                    }
                    else
                    {
                        QuickSortThread(arr, left, pivot - 1);
                    }

                    // дозвіл для створення нового потоку для правої частини
                    if (threadLimiter.Wait(0))
                    {
                        rightThread = new Thread(() =>
                        {
                            try
                            {
                                QuickSortThread(arr, pivot + 1, right);
                            }
                            finally
                            {
                                threadLimiter.Release();
                            }
                        });
                        rightThread.Start();
                    }
                    else
                    {
                        QuickSortThread(arr, pivot + 1, right);
                    }

                    if (leftThread != null)
                        leftThread.Join();
                    if (rightThread != null)
                        rightThread.Join();
                }
                else
                {
                    QuickSortThread(arr, left, pivot - 1);
                    QuickSortThread(arr, pivot + 1, right);
                }
            }
        }

        // Варіант 2: Quicksort із використанням Tasks
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

        // Варіант 3: Quicksort із використанням TaskFactory
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

        // Варіант 4: Quicksort із використанням Parallel.Invoke
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

        // Загальні методи Partition та Swap
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
    }
}
