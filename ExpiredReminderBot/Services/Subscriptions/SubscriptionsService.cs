using ExpiredReminderBot.Database;
using ExpiredReminderBot.Database.Rows;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services.Users;
using Microsoft.EntityFrameworkCore;
using NUlid;

namespace ExpiredReminderBot.Services.Subscriptions;

public class SubscriptionsService : ISubscriptionsService
{
    private readonly ExpiredReminderDbContext _dbContext;
    private readonly IUsersService _usersService;

    public SubscriptionsService(ExpiredReminderDbContext dbContext, IUsersService usersService)
    {
        _dbContext = dbContext;
        _usersService = usersService;
    }

    public async Task<Subscription> Create(string userId, string planId, DateTimeOffset endDate)
    {
        var plan = await GetPlan(planId);

        if (plan == null)
            throw new ArgumentNullException("Can't find this plan");

        var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(x => x.UserId == userId);
        if (subscription != null)
        {
            subscription.EndDate = subscription.EndDate.AddDays(plan.Days);
            return MapToDto(subscription);
        }

        var row = new SubscriptionRow(
            Ulid.NewUlid().ToString(),
            userId,
            planId,
            endDate.UtcDateTime,
            DateTimeOffset.UtcNow);

        await _dbContext.Subscriptions
            .AddAsync(row)
            .ConfigureAwait(false);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);

        await _usersService.Update(userId, new UpdateUserRequest(Subscription: true));

        return MapToDto(row);
    }

    public async Task<Subscription?> Get(string userId)
    {
        var row = await _dbContext.Subscriptions.FirstOrDefaultAsync(x => x.UserId == userId);

        if (row == null)
            return null;

        return MapToDto(row);

    }

    public async Task<ICollection<Subscription>> DeleteExpiredSubscriptions()
    {
        var rows = await 
            _dbContext.Subscriptions
            .Where(x => x.EndDate.Date <= DateTime.UtcNow.Date)
            .ToListAsync();

        _dbContext.Subscriptions.RemoveRange(rows);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);

        return rows.Select(MapToDto).ToList();
    }

    public async Task<SubscriptionPlan> CreatePlan(string userId, string title, int days, decimal price)
    {
        var row = new SubscriptionPlanRow(
            Ulid.NewUlid().ToString(),
            title, 
            days, 
            price);

        await _dbContext.SubscriptionPlans
            .AddAsync(row)
            .ConfigureAwait(false);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);

        return MapToDto(row);
    }

    public async Task<ICollection<SubscriptionPlan>> GetAllPlans()
    {
        var rows = await _dbContext.SubscriptionPlans.ToListAsync();

        if (!rows.Any())
            return null;

        return rows.Select(MapToDto).ToList();
    }

    public async Task<SubscriptionPlan> GetPlan(string planId)
    {
        var row = await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == planId);

        if (row == null)
            return null;

        return MapToDto(row);
    }

    private static Subscription MapToDto(SubscriptionRow row)
    {
        return new Subscription(
            row.Id, 
            row.UserId,
            row.EndDate,
            row.CreatedDate);
    }

    private static SubscriptionPlan MapToDto(SubscriptionPlanRow row)
    {
        return new SubscriptionPlan(
            row.Id,
            row.Title,
            row.Days,
            row.Price);
    }
}