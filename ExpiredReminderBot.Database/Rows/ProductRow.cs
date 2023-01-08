namespace ExpiredReminderBot.Database.Rows;

public class ProductRow
{
    public string Id { get; }
    public string Title { get; set; }
    public string GoldCode { get; set; }
    public string ShopId { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public DateTimeOffset CreatedDate { get; }

    public ShopRow Shop { get; set; }

    public ProductRow(string id, string title, string goldCode, string shopId, DateTimeOffset expiryDate, DateTimeOffset createdDate)
    {
        Id = id;
        Title = title;
        GoldCode = goldCode;
        ShopId = shopId;
        ExpiryDate = expiryDate;
        CreatedDate = createdDate;
    }
}