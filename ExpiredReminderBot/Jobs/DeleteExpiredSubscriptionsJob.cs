using ExpiredReminderBot.Services.Subscriptions;
using ExpiredReminderBot.Services.Users;
using Quartz;

namespace ExpiredReminderBot.Jobs;

[DisallowConcurrentExecution]
public class DeleteExpiredSubscriptionsJob : IJob
{
    private readonly ISubscriptionsService _subscriptionsService;
    private readonly IUsersService _usersService;

    public DeleteExpiredSubscriptionsJob(ISubscriptionsService subscriptionsService, IUsersService usersService)
    {
        _subscriptionsService = subscriptionsService;
        _usersService = usersService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var subs = await _subscriptionsService.DeleteExpiredSubscriptions();

        foreach (var sub in subs)
        {
            await _usersService.Update(sub.UserId, new(Subscription: false));
        }
    }
}