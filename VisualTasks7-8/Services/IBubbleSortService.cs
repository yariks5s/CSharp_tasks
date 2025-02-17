using System.Collections.Generic;
using System.Threading.Tasks;

namespace MvcTasksApp.Services
{
    public interface IBubbleSortService
    {
        IAsyncEnumerable<byte[]> GetBubbleSortAnimationAsync(int[] initialArray);
    }
}
