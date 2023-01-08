namespace ExpiredReminderBot.Models;

public class Product
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string GoldCode { get; set; }
    public string ShopId { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    public Product(string id, string title, string goldCode, string shopId, DateTimeOffset expiryDate, DateTimeOffset createdDate)
    {
        Id = id;
        Title = title;
        GoldCode = goldCode;
        ShopId = shopId;
        ExpiryDate = expiryDate;
        CreatedDate = createdDate;
    }
}