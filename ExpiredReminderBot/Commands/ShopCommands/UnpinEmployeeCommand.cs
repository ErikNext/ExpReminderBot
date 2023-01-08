using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Modes;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Shops;

namespace ExpiredReminderBot.Commands.ShopCommands;

public class UnpinEmployeeCommand : CommandBase
{
    private readonly IShopsService _shopsService;
    public override string Title => "🚷 Открепить сотрудника";
    public override string Key => "CheckShopEmployeesCallback";

    public UnpinEmployeeCommand(SenderService sender, IShopsService shopsService) : base(sender)
    {
        _shopsService = shopsService;
    }


    public override Task Execute(User user, string? data = default)
    {
        user.Mode = new UnpinEmployeeMode(Sender, _shopsService);
        return user.Mode.Execute(user, data);
    }
}

public class UnpinEmployeeMode : ModeBase
{
    private UnpinEmployeeStep _step;

    private Shop _shop;
    private readonly IShopsService _shopsService;

    public UnpinEmployeeMode(SenderService sender, IShopsService shopsService) : base(sender)
    {
        _shopsService = shopsService;
    }

    public override Task Execute(User user, string? data = default)
    {
        var handler = _step switch
        {
            UnpinEmployeeStep.Init => InitStep(user, data),
            UnpinEmployeeStep.Selection => SelectionStep(user, data),
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
                GetKeyboardElements());

            return;
        }

        var employees = await _shopsService.GetShopEmployees(_shop.Id);
        var keyboardElements = new List<InlineKeyboardElement>();

        foreach (var employee in employees)
        {
            keyboardElements.Add(new(employee.Username, employee.Id));
        }

        keyboardElements.AddRange(GetKeyboardElements());

        await SenderService.SendOrEditInlineKeyboard(user, "Выберите сотрудника которого хотите открепить",
            keyboardElements);

        _step++;
    }

    public async Task SelectionStep(User user, string data)
    {
        var employee = await _shopsService.TryUnpinEmployee(_shop.Id, data);

        if (employee == null)
        {
            await SenderService.SendOrEditInlineKeyboard(user, "Не удалось открепить сотрудника, обратитесь к разработчикам!",
                GetKeyboardElements());
            return;
        }

        await SenderService.SendOrEditInlineKeyboard(user, $"Сотрудник {employee.Username} был откреплен от магазина",
            GetKeyboardElements());
    }

    public ICollection<InlineKeyboardElement> GetKeyboardElements()
    {
        var elements = new List<InlineKeyboardElement>();

        var shopMenuCommand = StorageCommands.GetCommand(typeof(ShopMenuCommand));
        elements.Add(new(shopMenuCommand.Title, $"{shopMenuCommand.Key}:{_shop.Id}"));

        return elements;
    }
}

public enum UnpinEmployeeStep : byte
{
    Init,
    Selection,
    Action
}