using ExpiredReminderBot.Models;

namespace ExpiredReminderBot.Services.Subscriptions;

public interface ISubscriptionsService
{
    Task<Subscription> Create(string userId, string planId, DateTimeOffset endDate);

    Task<Subscription?> Get(string userId);

    Task<ICollection<Subscription>> DeleteExpiredSubscriptions();

    Task<SubscriptionPlan> CreatePlan(string userId, string title, int days, decimal price);

    Task<ICollection<SubscriptionPlan>> GetAllPlans();

    Task<SubscriptionPlan> GetPlan(string planId);
}