namespace ExpiredReminderBot.Database.Rows;

public class SubscriptionPlanRow
{
    public string Id { get; }
    public string Title { get; set; }
    public int Days { get; set; }
    public decimal Price { get; set; }

    public List<SubscriptionRow> Subscriptions { get; set; } = new();

    public SubscriptionPlanRow(string id, string title, int days, decimal price)
    {
        Id = id;
        Title = title;
        Days = days;
        Price = price;
    }
}