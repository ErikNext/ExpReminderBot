using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Modes;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Shops;

namespace ExpiredReminderBot.Commands;

public class MyShopsCommand : CommandBase
{
    private readonly IShopsService _shopsService;
    public override string Title => "Мои магазины";
    public override string Key => "MyShopsCallback";

    public MyShopsCommand(SenderService sender, IShopsService shopsService) : base(sender)
    {
        _shopsService = shopsService;
    }

    public override async Task Execute(User user, string? data = default)
    {
        if (!user.Subscription)
        {
            await Sender.SendSubOffer(user);
            return;
        }

        var shops = await _shopsService.GetUserShops(user.Id);

        if (!shops.Any())
        {
            await Sender.SendOrEditInlineKeyboard(user, "У вас нет созданного магазина",
                GetAvailableCommands(user).ToKeyboardElements());

            return;
        }

        List<InlineKeyboardElement> elements = new();

        foreach (var shop in shops)
            elements.Add(new(shop.Title, shop.Id));

        elements.AddRange(GetAvailableCommands(user).ToKeyboardElements());

        await Sender.SendOrEditInlineKeyboard(user, "Ваши магазины: ", elements);
        user.Mode = new SelectShopMode(Sender, _shopsService);
    }
}