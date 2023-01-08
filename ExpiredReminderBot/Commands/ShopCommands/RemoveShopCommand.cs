using System.Collections.Immutable;
using System.Globalization;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Modes;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Shops;
using System.Reflection;
using ExpiredReminderBot.Models;

namespace ExpiredReminderBot.Commands.ShopCommands;

public class RemoveShopCommand : CommandBase
{
    private readonly IShopsService _shopService;
    public override string Title => "❌ Удалить магазин";
    public override string Key => "RemoveShopCallback";

    public RemoveShopCommand(SenderService sender, IShopsService shopService) : base(sender)
    {
        _shopService = shopService;
    }

    public override async Task Execute(User user, string? data = default)
    {
        user.Mode = new RemoveShopMode(Sender, _shopService);
        await user.Mode.Execute(user, data);
    }
}

public class RemoveShopMode : ModeBase
{
    private RemoveShopStep _step;

    private Shop _shop;
    private readonly IShopsService _shopsService;

    private const string Remove = "Удалить";
    private const string Cancel = "Отменить";

    public RemoveShopMode(SenderService sender, IShopsService shopsService) : base(sender)
    {
        _shopsService = shopsService;
    }

    public override Task Execute(User user, string? data = default)
    {
        var handler = _step switch
        {
            RemoveShopStep.Init => InitStep(user, data),
            RemoveShopStep.Confirmation => ConfirmationStep(user, data),
            _ => throw new Exception()
        };

        return handler;
    }

    public async Task InitStep(User user, string data)
    {
        _shop = await _shopsService.Get(data);

        if (_shop == null)
        {
            await SenderService.SendOrEditInlineKeyboard(user, "Не удалось найти магазин",
                GetKeyboardElements(true));

            return;
        }

        await SenderService.SendOrEditInlineKeyboard(user,
            "Внимание❗️\nВы уверены, что хотите безвозвратно удалить магазин и всю продукцию?", GetAvailableOperations());

        _step++;
    }

    public async Task ConfirmationStep(User user, string data)
    {
        user.Mode = null;

        switch (data)
        {
            case Remove:
                try
                {
                    await _shopsService.Delete(user.Id, _shop.Id);
                    await SenderService.SendOrEditInlineKeyboard(user,
                        "Магазин был успешно удален!", GetKeyboardElements(true));
                }
                catch
                {
                    await SenderService.SendOrEditInlineKeyboard(user,
                        "Не удалось удалить магазин! Обратиесь к разработчикам!", GetKeyboardElements(true));
                }
                break;
            
            case Cancel:
                await SenderService.SendOrEditInlineKeyboard(user,
                    "Операция отменена!", GetKeyboardElements(false));
                break;
        }
    }

    public ICollection<InlineKeyboardElement> GetAvailableOperations()
    {
        return new List<InlineKeyboardElement>()
        {
            new(Remove, Remove),
            new(Cancel, Cancel)
        };
    }

    public ICollection<InlineKeyboardElement> GetKeyboardElements(bool mainMenu)
    {
        var elements = new List<InlineKeyboardElement>();

        if (mainMenu)
        {
            var mainMenuCommand = StorageCommands.GetCommand(typeof(MainMenuCommand));
            elements.Add(new(mainMenuCommand.Title, mainMenuCommand.Key));
        }
        else
        {
            var shopMenuCommand = StorageCommands.GetCommand(typeof(ShopMenuCommand));
            elements.Add(new(shopMenuCommand.Title, $"{shopMenuCommand.Key}:{_shop.Id}"));
        }

        return elements;
    }
}

public enum RemoveShopStep : byte
{
    Init,
    Confirmation
}