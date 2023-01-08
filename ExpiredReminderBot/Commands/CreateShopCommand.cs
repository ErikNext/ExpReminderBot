using ExpiredReminderBot.Models;
using ExpiredReminderBot.Modes;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Shops;

namespace ExpiredReminderBot.Commands;

public class CreateShopCommand : CommandBase
{
    private readonly IShopsService _shopsService;
    public override string Title => "Создать магазин";
    public override string Key => "CreateShopCallback";

    public CreateShopCommand(SenderService sender, IShopsService shopsService) : base(sender)
    {
        _shopsService = shopsService;
    }

    public override Task Execute(User user, string? data = default)
    {
        if (!user.Subscription)
            return Sender.SendSubOffer(user);

        user.Mode = new CreateShopMode(Sender, _shopsService);
        user.Mode.Execute(user, data);
        return Task.CompletedTask;
    }
}