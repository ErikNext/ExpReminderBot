using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Modes;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Products;
using ExpiredReminderBot.Services.Shops;

namespace ExpiredReminderBot.Commands.ShopCommands;

public class CreateProductCommand : CommandBase
{
    private readonly IShopsService _shopsService;
    private readonly IProductsService _productsService;
    public override string Title => "Добавить продукт";
    public override string Key => "CreateProductCommand";

    public CreateProductCommand(SenderService sender, IShopsService shopsService, IProductsService productsService) : base(sender)
    {
        _shopsService = shopsService;
        _productsService = productsService;
    }

    public override async Task Execute(User user, string? data = default)
    {
        var shop = await _shopsService.Get(data);

        if (shop == null)
        {
            await Sender.SendOrEditInlineKeyboard(user, "Ошибка! Магазин не найден",
                GetAvailableCommands(user).ToKeyboardElements());
            return;
        }

        user.Mode = new CreateProductMode(Sender, shop, _productsService);
        await user.Mode.Execute(user);
    }

    public override IEnumerable<CommandBase> GetAvailableCommands(User user)
    {
        var commands = new List<CommandBase>()
        {
            StorageCommands.GetCommand(typeof(ShopMenuCommand))
        };

        return commands;
    }
}