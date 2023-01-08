using ExpiredReminderBot.Commands;
using ExpiredReminderBot.Commands.AdminCommands;
using ExpiredReminderBot.Commands.ShopCommands;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Shops;

namespace ExpiredReminderBot.Modes;

public class SelectShopMode : ModeBase
{
    private readonly IShopsService _shopsService;
    private Shop _shop { get; set; }

    public SelectShopMode(SenderService sender, IShopsService shopsService) : base(sender)
    {
        _shopsService = shopsService;
    }

    public override async Task Execute(User user, string? data = default)
    {
        user.Mode = null;
        _shop = await _shopsService.Get(data);

        if (_shop == null)
        {
            await SenderService.SendOrEditInlineKeyboard(user, "Магазин не найден", GetAvailableCommands(user).ToKeyboardElements());
            return;
        }

        await SenderService.SendOrEditInlineKeyboard(user, $"Магазин '{_shop.Title}'", GetAvailableOperations(user, _shop.Id));
    }

    public ICollection<InlineKeyboardElement> GetAvailableOperations(User user, string shopId)
    {
        var createProductCommand = StorageCommands.GetCommand(typeof(CreateProductCommand));
        var mainMenuCommand = StorageCommands.GetCommand(typeof(MainMenuCommand));
        var moreCommand = StorageCommands.GetCommand(typeof(MoreCommand));

        if (user.Id == _shop.DirectorId)
        {
            return new List<InlineKeyboardElement>()
            {
                new(createProductCommand.Title, $"{createProductCommand.Key}:{shopId}"),
                new(moreCommand.Title, $"{moreCommand.Key}:{shopId}"),
                new(mainMenuCommand.Title, mainMenuCommand.Key)
            };
        }

        return new List<InlineKeyboardElement>()
        {
            new(createProductCommand.Title, $"{createProductCommand.Key}:{shopId}"),
            new(mainMenuCommand.Title, mainMenuCommand.Key)
        };

    }
}