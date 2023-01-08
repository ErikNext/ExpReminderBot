namespace ExpiredReminderBot.Database.Rows;

public class ShopRow
{
    public string Id { get; }
    public string Title { get; set; }
    public string DirectorId { get; set; }

    public List<ProductRow> Products { get; } = new();
    public ShopAccessRow ShopAccess { get; set; }

    public ShopRow(string id, string title, string directorId)
    {
        Id = id;
        Title = title;
        DirectorId = directorId;
    }
}