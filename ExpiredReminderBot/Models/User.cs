using ExpiredReminderBot.Commands;
using ExpiredReminderBot.Modes;
using Telegram.Bot.Types;

namespace ExpiredReminderBot.Models;

public class User
{
    public string Id { get; set; }
    public long TelegramId { get; }
    public string Username { get; }
    public decimal Balance { get; set; }
    public bool Subscription { get; set; }
    public Message? LastSendMessage { get; set; }
    public Message? LastReceivedMessage { get; set; }
    public CommandBase? LastCommand { get; set; }
    public ModeBase? Mode { get; set; }
    
    public User(string id, long telegramId, string username, decimal balance, bool subscription)
    {
        Id = id;
        TelegramId = telegramId;
        Username = username;
        Balance = balance;
        Subscription = subscription;
    }
}