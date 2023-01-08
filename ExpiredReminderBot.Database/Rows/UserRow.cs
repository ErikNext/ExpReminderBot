namespace ExpiredReminderBot.Database.Rows;

public class UserRow
{
    public string Id { get; }
    public string Username { get; set; }
    public decimal Balance { get; set; }
    public long TelegramId { get; set; }
    public bool Subscription { get; set; }
    public DateTimeOffset CreatedDate { get; }

    public List<ShopAccessRow> ShopAccessRows { get; } = new();

    public UserRow(
        string id,
        string username,
        decimal balance,
        long telegramId,
        DateTimeOffset createdDate) 
    {
        Id = id;
        Username = username;
        Balance = balance;
        TelegramId = telegramId; 
        CreatedDate = createdDate;
        Balance = balance;
    }
}