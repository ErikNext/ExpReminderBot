namespace ExpiredReminderBot.Database.Rows;

public class SubscriptionRow
{
    public string Id { get; }
    public string UserId { get; set; }
    public string PlanId { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public SubscriptionPlanRow Plan { get; set; }

    public SubscriptionRow(string id, string userId, string planId, DateTimeOffset endDate, DateTimeOffset createdDate)
    {
        Id = id;
        UserId = userId;
        PlanId = planId;
        EndDate = endDate;
        CreatedDate = createdDate;
    }
}