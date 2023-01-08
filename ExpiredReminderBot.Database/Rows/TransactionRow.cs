using ExpiredReminderBot.Database.Items;

namespace ExpiredReminderBot.Database.Rows;

public class TransactionRow
{
    public string Id { get; }
    public string UserId { get; }
    public decimal Amount { get; }
    public TransactionType Type { get; }
    public DateTimeOffset CreatedDate { get; }

    public TransactionRow(string id, string userId, decimal amount, TransactionType type, DateTimeOffset createdDate)
    {
        Id = id;
        UserId = userId;
        Amount = amount;
        Type = type;
        CreatedDate = createdDate;
    }
}