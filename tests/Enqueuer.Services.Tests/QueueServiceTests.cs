using System.Linq;
using System.Threading.Tasks;
using Enqueuer.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Enqueuer.Services.Tests;

public class QueueServiceTests
{
    [Fact]
    public async Task EnqueueOnFirstAvailablePositionAsync()
    {
        using (var context = new EnqueuerContext(new DbContextOptionsBuilder<EnqueuerContext>().UseSqlite("DataSource=enqueuer.db").Options))
        {
            var firstAvailablePosition = context.Positions
                .SelectMany(p => context.QueueMembers.Where(m => m.QueueId == 142 && m.Position == p.Value).DefaultIfEmpty(),
                (position, member) => new { position, member })
                .First(m => m.member == null).position;
                //.First(m => m)
                //.ToList();
        }
    }
}
