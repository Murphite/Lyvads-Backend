using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Lyvads.Domain.Enums;
using Lyvads.Infrastructure.Persistence;


namespace Lyvads.Application.Implementions;

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
                var requests = await dbContext.CollaborationRequests
                    .Where(r => r.Status == RequestStatus.Pending &&
                                DateTimeOffset.UtcNow - r.UpdatedAt > TimeSpan.FromDays(3))
                    .ToListAsync(stoppingToken);

                foreach (var request in requests)
                {
                    request.Status = RequestStatus.Completed;
                    dbContext.CollaborationRequests.Update(request);
                }

                // Save changes to the database
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
