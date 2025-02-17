using System.Threading.Tasks;
using MvcTasksApp.Models.Sorting;
using System.Collections.Generic;

namespace MvcTasksApp.Services
{
    public class BubbleSortService : IBubbleSortService
    {
        public async IAsyncEnumerable<byte[]> GetBubbleSortAnimationAsync(int[] initialArray)
        {
            var animator = new BubbleSortAnimator(initialArray);
            await foreach (var frame in animator.AnimateAsync())
            {
                yield return frame;
            }
        }
    }
}
