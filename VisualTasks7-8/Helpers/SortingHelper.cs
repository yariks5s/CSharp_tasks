using Microsoft.AspNetCore.Http;
using MvcTasksApp.Services;
using System;
using System.Threading.Tasks;

namespace MvcTasksApp.Helpers
{
    public static class SortingHelper
    {
        public static async Task StreamBubbleSort(IBubbleSortService bubbleSortService, HttpResponse response)
        {
            // Налаштовуємо заголовки для стрімінгу
            response.ContentType = "multipart/x-mixed-replace; boundary=frame";
            response.Headers["Cache-Control"] = "no-cache";
            response.Headers["Connection"] = "keep-alive";

            // Генерація випадкового масиву
            Random rnd = new Random();
            int numBars = 30;
            int[] array = new int[numBars];
            for (int i = 0; i < numBars; i++)
            {
                array[i] = rnd.Next(10, 311);
            }

            await foreach (var frame in bubbleSortService.GetBubbleSortAnimationAsync(array))
            {
                await WriteImageFrame(response, frame);
            }
        }

        private static async Task WriteImageFrame(HttpResponse response, byte[] imageBytes)
        {
            string boundary = "--frame\r\n";
            await response.WriteAsync(boundary);
            await response.WriteAsync("Content-Type: image/png\r\n");
            await response.WriteAsync($"Content-Length: {imageBytes.Length}\r\n\r\n");
            await response.Body.WriteAsync(imageBytes, 0, imageBytes.Length);
            await response.WriteAsync("\r\n");
            await response.Body.FlushAsync();
        }
    }
}
