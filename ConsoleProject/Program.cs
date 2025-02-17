using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    // добавлено: константа для максимального числа потоків та SemaphoreSlim для їх обмеження
    private const int MAX_THREADS = 4;
    private static SemaphoreSlim threadLimiter = new SemaphoreSlim(MAX_THREADS);

    static void Main(string[] args)
    {


        Console.WriteLine("Будь ласка введіть числа через кому:");
        string input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Немає вхідних даних");
            return;
        }
        
        int[] numbers = input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => int.Parse(s.Trim()))
                             .ToArray();

        Console.WriteLine("\nВиберіть метод сортування:");
        Console.WriteLine("1 - Quicksort із використанням Thread");
        Console.WriteLine("2 - Quicksort із використанням Tasks");
        Console.WriteLine("3 - Quicksort із використанням TaskFactory");
        Console.WriteLine("4 - Quicksort із використанням Parallel");
        Console.Write("Ваш вибір: ");


        string choice = Console.ReadLine();

        // Склонуємо масив, щоб оригінал не змінювався
        int[] arr = (int[])numbers.Clone();
        Stopwatch sw = Stopwatch.StartNew();


        switch (choice)
        {
            case "1":
                QuickSortThread(arr, 0, arr.Length - 1);
                break;
            case "2":
                QuickSortTasks(arr, 0, arr.Length - 1).Wait();
                break;
            case "3":
                QuickSortTaskFactory(arr, 0, arr.Length - 1, new TaskFactory()).Wait();
                break;
            case "4":
                QuickSortParallel(arr, 0, arr.Length - 1);
                break;
            default:
                Console.WriteLine("Немає такої опції");
                return;
        }

        sw.Stop();

        Console.WriteLine("\nВідсортований масив: " + string.Join(", ", arr));

        Console.WriteLine($"Час виконання: {sw.ElapsedMilliseconds} мс");
    }

    // Quicksort із використанням класу Thread з обмеженням кількості потоків
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

                // Попробувати отримати дозвіл для створення нового потоку для лівої частини
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
                    QuickSortThread(arr, pivot + 1, right);

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

    // Quicksort із використанням Tasks
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

    // Quicksort із використанням TaskFactory
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

    // Quicksort із використанням Parallel
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

    // для Quicksort
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
