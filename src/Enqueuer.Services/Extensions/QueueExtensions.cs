using System.Linq;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Extensions;

public static class QueueExtensions
{
    public static int[] GetAvailablePositions(this Queue queue, int numberOfPositions)
    {
        var availablePositions = new int[numberOfPositions];
        var reservedPositions = queue.Members.OrderBy(m => m.Position).Select(m => m.Position).ToList();
        int currentPosition = 0, currentIndex = 0;
        while (currentIndex < numberOfPositions)
        {
            currentPosition++;
            if (reservedPositions.Contains(currentPosition))
            {
                continue;
            }

            availablePositions[currentIndex] = currentPosition;
            currentIndex++;
        }

        return availablePositions;
    }
}
