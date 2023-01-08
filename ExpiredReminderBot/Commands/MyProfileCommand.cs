using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Subscriptions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Telegram.Bot.Types.Enums;

namespace ExpiredReminderBot.Commands;

public class MyProfileCommand : CommandBase
{
    private readonly ISubscriptionsService _subscriptionsService;
    public override string Title => "👤 Мой профиль";
    public override string Key => "MyProfileCallback";

    public MyProfileCommand(SenderService sender, ISubscriptionsService subscriptionsService) : base(sender)
    {
        _subscriptionsService = subscriptionsService;
    }

    public override async Task Execute(User user, string? data = default)
    {
        var message = $"Username: {user.Username}\n" +
                            $"ID: `{user.Id}`\n" +
                            $"Баланс: {user.Balance} руб.\n" +
                            $"Подписка: ";

        var subscription = await _subscriptionsService.Get(user.Id);

        if (subscription == null)
            message += "не активна";
        else
            message += $"до {subscription.EndDate.Date.ToShortDateString()}";


        await Sender.SendOrEditInlineKeyboard(user, message, GetAvailableElements(), ParseMode.Markdown);
    }

    public ICollection<InlineKeyboardElement> GetAvailableElements()
    {
        var commandSub = StorageCommands.GetCommand(typeof(BuySubscription));
        var commandMenu = StorageCommands.GetCommand(typeof(MainMenuCommand));

        return new List<InlineKeyboardElement>()
        {
            new(commandSub.Title, commandSub.Key),
            new("💸 Пополнить баланс", "AddBalance", "https://t.me/eriknext"),
            new (commandMenu.Title, commandMenu.Key),
        };
    }
}