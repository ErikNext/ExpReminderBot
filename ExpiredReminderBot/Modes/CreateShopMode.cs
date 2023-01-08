using ExpiredReminderBot.Commands;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Shops;
using Telegram.Bot.Requests;

namespace ExpiredReminderBot.Modes;

public class CreateShopMode : ModeBase
{
    private readonly IShopsService _shopsService;
    private CreateShopStep _step { get; set; }

    public CreateShopMode(SenderService sender, IShopsService shopsService) : base(sender)
    {
        _shopsService = shopsService;
    }

    public override Task Execute(User user, string? data = default)
    {
        var handler = _step switch
        {
            CreateShopStep.Init => InitStep(user),
            CreateShopStep.SetTitle => SetTitleStep(user, data),
            _ => throw new Exception()
        };

        return handler;
    }

    public async Task InitStep(User user)
    {
        await SenderService.SendOrEditInlineKeyboard(user, "Введите название магазина",
            GetAvailableCommands(user).ToKeyboardElements());

        _step++;
    }

    public async Task SetTitleStep(User user, string data)
    {
        await SenderService.RemoveMessage(user, user.LastSendMessage.MessageId);

        if (String.IsNullOrEmpty(data))
        {
            await SenderService.SendOrEditInlineKeyboard(user, "Ошибка! Введите корректное название!",
                GetAvailableCommands(user).ToKeyboardElements());
            return;
        }

        await _shopsService.Create(new(data, user.Id));

        await SenderService.SendOrEditInlineKeyboard(user, $"Магазин '{data}' успешно создан!",
            GetAvailableCommands(user).ToKeyboardElements());

        user.Mode = null;
    }
}

public enum CreateShopStep : byte
{
    Init,
    SetTitle
}