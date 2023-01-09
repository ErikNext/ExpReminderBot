using System.Globalization;
using ExpiredReminderBot.Commands;
using ExpiredReminderBot.Commands.ShopCommands;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Products;
using Microsoft.Extensions.Primitives;
using Quartz.Xml.JobSchedulingData20;

namespace ExpiredReminderBot.Modes;

public class CreateProductMode : ModeBase
{
    private const string Save = "Сохранить";
    private const string Cancel = "Отменить";

    private CreateProductRequest _createProductRequest;
    private CreateProductStep _step;
    private Shop _shop;

    private readonly IProductsService _productsService;


    public CreateProductMode(SenderService sender, Shop shop, IProductsService productsService) : base(sender)
    {
        _shop = shop;
        _productsService = productsService;
        _createProductRequest = new CreateProductRequest();
    }

    public override Task Execute(User user, string? data = default)
    {
        var handler = _step switch
        {
            CreateProductStep.Init => InitStep(user),
            CreateProductStep.SetTitle => SetTitleStep(user, data),
            CreateProductStep.SetGoldCode => SetGoldCode(user, data),
            CreateProductStep.SetExpiryDate => SetExpiryDate(user, data),
            CreateProductStep.Confirmation => Confirmation(user, data),
            _ => throw new Exception()
        };

        return handler;
    }

    public async Task InitStep(User user)
    {
        await SenderService.SendOrEditInlineKeyboard(user, "Введите название продукта",
            GetKeyboardElements());

        _step++;
    }

    public async Task SetTitleStep(User user, string data)
    {
        if (String.IsNullOrEmpty(data))
        {
            await SenderService.SendInlineKeyboard(user, "Ошибка! Введите корректное название!",
                GetKeyboardElements());
            return;
        }

        _createProductRequest.Title = data;
        _step++;

        await SenderService.SendInlineKeyboard(user, "Введите gold code",
            GetKeyboardElements());
    }

    public async Task SetGoldCode(User user, string data)
    {
        //ToDo: сделать проверку голд кода по каким либо параметрам

        if (String.IsNullOrEmpty(data))
        {
            await SenderService.SendInlineKeyboard(user, "Ошибка! Введите корректный gold code!",
                GetKeyboardElements());
            return;
        }

        _createProductRequest.GoldCode = data;
        _step++;

        await SenderService.SendInlineKeyboard(user, "Введите дату выхода из срока или кол-во дней до выхода из срока",
            GetKeyboardElements());
    }

    public async Task SetExpiryDate(User user, string data)
    {
        var culture = CultureInfo.CreateSpecificCulture("ru-RU");

        if (!DateTimeOffset.TryParse(data, culture, DateTimeStyles.None, out var expiryDate))
        {
            await SenderService.SendInlineKeyboard(user, "Ошибка! Введите корректную дату",
                GetKeyboardElements());
            return;
        }

        if (expiryDate < DateTimeOffset.UtcNow)
        {
            await SenderService.SendInlineKeyboard(user, "Ошибка! Указанная дата не должна быть меньше текущей",
                GetKeyboardElements());
            return;
        }

        _createProductRequest.ExpiryDate = expiryDate;
        _step++;

        await SenderService.SendInlineKeyboard(user, $"Название: {_createProductRequest.Title}\n" +
                                                            $"Gold key: {_createProductRequest.GoldCode}\n" +
                                                            $"Дата выхода из срока: {_createProductRequest.ExpiryDate.DateTime.ToString("dd.MM.yyyy")}", 
            GetAvailableOperations());
    }

    public async Task Confirmation(User user, string data)
    {
        user.Mode = null;

        switch (data)
        {
            case Save:
                var createProductCommand = StorageCommands.GetCommand(typeof(CreateProductCommand));
                var commands = GetKeyboardElements();
                commands.Add(new("Добавить еще", $"{createProductCommand.Key}:{_shop.Id}"));

                var product = await _productsService.Create(_createProductRequest, _shop.Id);
                await SenderService.SendOrEditInlineKeyboard(user, $"Продукт '{product.Title}' был добавлен!",
                    commands);
                break;

            case Cancel:
                await SenderService.SendOrEditInlineKeyboard(user, "Операция отменена!",
                    GetKeyboardElements());
                break;

            default:
                await SenderService.SendOrEditInlineKeyboard(user, "Не удалось создать продукт!",
                    GetKeyboardElements());
                break;
        }
    }

    public ICollection<InlineKeyboardElement> GetAvailableOperations()
    {
        return new List<InlineKeyboardElement>()
        {
            new(Save, Save),
            new(Cancel, Cancel)
        };
    }

    public ICollection<InlineKeyboardElement> GetKeyboardElements()
    {
        var shopMenuCommand = StorageCommands.GetCommand(typeof(ShopMenuCommand));

        return new List<InlineKeyboardElement>()
        {
            new("В меню магазина", $"{shopMenuCommand.Key}:{_shop.Id}")
        };
    }
}

public enum CreateProductStep : byte
{
    Init,
    SetTitle,
    SetGoldCode,
    SetExpiryDate,
    Confirmation
}