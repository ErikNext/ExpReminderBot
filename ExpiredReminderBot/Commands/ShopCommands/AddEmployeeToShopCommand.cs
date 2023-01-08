using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Modes;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Shops;
using ExpiredReminderBot.Services.Users;
using System.Reflection;

namespace ExpiredReminderBot.Commands.ShopCommands;

public class AddEmployeeToShopCommand : CommandBase
{
    private readonly IShopsService _shopsService;
    private readonly IUsersService _usersService;
    public override string Title => "👨🏻‍💼 Добавить сотрудника";
    public override string Key => "AddManagerToShopCallback";

    public AddEmployeeToShopCommand(SenderService sender, IShopsService shopsService, IUsersService usersService) : base(sender)
    {
        _shopsService = shopsService;
        _usersService = usersService;
    }

    public override async Task Execute(User user, string? data = default)
    {
        user.Mode = new AddEmployeeToShopMode(Sender, _shopsService, _usersService);
        await user.Mode.Execute(user, data);
    }
}

public class AddEmployeeToShopMode : ModeBase
{
    private readonly IShopsService _shopsService;
    private readonly IUsersService _usersService;

    private Shop _shop;
    private AddManagerToShopStep _step;

    public AddEmployeeToShopMode(SenderService sender, IShopsService shopsService, IUsersService usersService) : base(sender)
    {
        _shopsService = shopsService;
        _usersService = usersService;
    }

    public override Task Execute(User user, string? data = default)
    {
        var handler = _step switch
        {
            AddManagerToShopStep.Init => InitStep(user, data),
            AddManagerToShopStep.SetManagerId => SetManagerIdStep(user, data),
            _ => throw new Exception()
        };

        return handler;
    }

    public async Task InitStep(User user, string data)
    {
        _shop = await _shopsService.Get(data);

        if (_shop == null)
        {
            await SenderService.SendOrEditInlineKeyboard(user, "Ошибка! Магазин не найден",
                GetAvailableCommands(user).ToKeyboardElements());
            return;
        }


        await SenderService.SendOrEditInlineKeyboard(user, "Отправьте ID сотрудника которому хотите предоставить доступ. Свой ID он может узнать в меню бота!",
            GetKeyboardElements());

        _step++;
    }

    public async Task SetManagerIdStep(User user, string data)
    {
        await SenderService.RemoveMessage(user, user.LastSendMessage.MessageId);
        user.Mode = null;

        var manager = await _usersService.Get(data);

        if (manager == null)
        {
            await SenderService.SendOrEditInlineKeyboard(user,
                "Пользователь не найден!", GetKeyboardElements());
            return;
        }

        if (!await _shopsService.TryAttachToShop(user.Id, manager.Id, _shop.Id))
        {
            await SenderService.SendOrEditInlineKeyboard(user,
                "Не удалось прикрепить сотрудника к магазину!", GetKeyboardElements());
        }

        await SenderService.SendOrEditInlineKeyboard(user,
            "Сотрудник успешно прикреплен к магазину!", GetKeyboardElements());
    }

    public ICollection<InlineKeyboardElement> GetKeyboardElements()
    {
        var elements = new List<InlineKeyboardElement>();

        var shopMenuCommand = StorageCommands.GetCommand(typeof(ShopMenuCommand));
        elements.Add(new(shopMenuCommand.Title, $"{shopMenuCommand.Key}:{_shop.Id}"));

        return elements;
    }
}

public enum AddManagerToShopStep : byte
{
    Init,
    SetManagerId
}