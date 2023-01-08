namespace ExpiredReminderBot.Models;

public class Shop
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string DirectorId { get; set; }

    public Shop(string id, string title, string directorId)
    {
        Id = id;
        Title = title;
        DirectorId = directorId;
    }
}
