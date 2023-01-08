namespace ExpiredReminderBot.Services.Products
{
    public class CreateProductRequest
    {
        public string Title { get; set; }
        public string GoldCode { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
    }
}
