using System.Linq;
using Enqueuer.Persistence.Models;

namespace Enqueuer.Services.Extensions;

public static class QueueExtensions
{
    public static int[] GetAvailablePositions(this Queue queue, int numberOfPositions)
    {
        var availablePositions = new int[numberOfPositions];
        var reservedPositions = queue.Members.OrderBy(m => m.Position).Select(m => m.Position);
        int currentPosition = 1, currentIndex = 0;
        foreach (var position in reservedPositions)
        {
            if (position != currentPosition)
            {
                availablePositions[currentIndex] = position;
                currentIndex++;
            }

            if (currentIndex >= numberOfPositions)
            {
                return availablePositions;
            }

            currentPosition++;
        }

        while (currentIndex < numberOfPositions)
        {
            availablePositions[currentIndex] = currentPosition;
            currentIndex++;
            currentPosition++;
        }

        return availablePositions;
    }
}
