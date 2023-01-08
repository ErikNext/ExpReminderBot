using ExpiredReminderBot.Database.Items;

namespace ExpiredReminderBot.Models;

public record Subscription(
    string Id, 
    string UserId,
    DateTimeOffset EndDate,
    DateTimeOffset CreatedDate);