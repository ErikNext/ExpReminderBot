using System.Security.Cryptography.X509Certificates;

namespace ExpiredReminderBot.Models;

public record SubscriptionPlan(string Id, string Title, int Days, decimal Price);