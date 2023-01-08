using ExpiredReminderBot.Database.Items;

namespace ExpiredReminderBot.Models;

public record Transaction(
    string Id,
    string UserId,
    decimal Amount, 
    TransactionType Type, 
    DateTimeOffset CreatedDate);