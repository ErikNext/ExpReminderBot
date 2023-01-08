namespace ExpiredReminderBot.Database.Rows;

public class ShopAccessRow
{
    public string Id { get; }
    public string ShopId { get; set; }
    public string UserId { get; set; }

    public ShopRow Shop { get; set; }
    public UserRow User { get; set; }

    public ShopAccessRow(string id, string shopId, string userId)
    {
        Id = id;
        ShopId = shopId;
        UserId = userId;
    }
}