using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Modes;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Shops;

namespace ExpiredReminderBot.Commands;

public class ShopMenuCommand : CommandBase
{
    private readonly IShopsService _shopsService;
    public override string Title => "Меню магазина";
    public override string Key => "ShopMenuCallback";
    
    public ShopMenuCommand(SenderService sender, IShopsService shopsService) : base(sender)
    {
        _shopsService = shopsService;
    }

    public override async Task Execute(User user, string? data = default)
    {
        user.Mode = new SelectShopMode(Sender, _shopsService);
        await user.Mode.Execute(user, data);
    }
}