using ExpiredReminderBot.Commands.AdminCommands;
using ExpiredReminderBot.Commands.ShopCommands;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Shops;
using Telegram.Bot.Types;
using User = ExpiredReminderBot.Models.User;

namespace ExpiredReminderBot.Commands;

public class MoreCommand : CommandBase
{
    private readonly IShopsService _shopsService;
    public override string Title => "Еще..";
    public override string Key => "MoreCallback";


    public MoreCommand(SenderService sender, IShopsService shopsService) : base(sender)
    {
        _shopsService = shopsService;
    }

    public override async Task Execute(User user, string? data = default)
    {
        var shop = await _shopsService.Get(data);

        if (shop == null)
        {
            await Sender.SendOrEditInlineKeyboard(user, "Не удалось найти магазин",
                GetAvailableCommands(user).ToKeyboardElements());

            return;
        }

        await Sender.SendOrEditInlineKeyboard(user, $"Магазин {shop.Title}",
            GetAvailableOperations(user, shop));
    }

    public ICollection<ICollection<InlineKeyboardElement>> GetAvailableOperations(User user, Shop shop)
    {
        var shopMenuCommand = StorageCommands.GetCommand(typeof(ShopMenuCommand));
        var removeShopCommand = StorageCommands.GetCommand(typeof(RemoveShopCommand));
        var unpinEmployeeCommand = StorageCommands.GetCommand(typeof(UnpinEmployeeCommand));
        var addManagerToShopCommand = StorageCommands.GetCommand(typeof(AddEmployeeToShopCommand));

        if (user.Id == shop.DirectorId)
        {
            var elements = new List<ICollection<InlineKeyboardElement>>()
            {
                new List<InlineKeyboardElement>()
                {
                    new(addManagerToShopCommand.Title, $"{addManagerToShopCommand.Key}:{shop.Id}")
                },
                new List<InlineKeyboardElement>()
                {
                    new(unpinEmployeeCommand.Title, $"{unpinEmployeeCommand.Key}:{shop.Id}")
                },
                new List<InlineKeyboardElement>()
                {
                    new(removeShopCommand.Title, $"{removeShopCommand.Key}:{shop.Id}")
                },
                new List<InlineKeyboardElement>()
                {
                    new("◀️ Назад", $"{shopMenuCommand.Key}:{shop.Id}")
                },
            };

            return elements;
        }

        return new List<ICollection<InlineKeyboardElement>>();
    }
}