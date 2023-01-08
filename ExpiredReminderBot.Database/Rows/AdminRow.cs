namespace ExpiredReminderBot.Database.Rows;

public class AdminRow
{
    public string Id { get; }
    public string UserId { get; }

    public AdminRow(string id, string userId)
    {
        Id = id;
        UserId = userId;
    }
}