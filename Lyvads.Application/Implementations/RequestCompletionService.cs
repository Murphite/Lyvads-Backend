using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Lyvads.Domain.Enums;
using Lyvads.Infrastructure.Persistence;


namespace Lyvads.Application.Implementations;

public class RequestCompletionService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public RequestCompletionService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_checkInterval, stoppingToken);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var requests = await dbContext.Collaborations
                    .Where(r => r.Status == CollaborationStatus.Pending &&
                                DateTimeOffset.UtcNow - r.UpdatedAt > TimeSpan.FromDays(3))
                    .ToListAsync(stoppingToken);

                foreach (var request in requests)
                {
                    request.Status = CollaborationStatus.Completed;
                    dbContext.Collaborations.Update(request);
                }

                // Save changes to the database
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
