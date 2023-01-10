using ExpiredReminderBot.Database.Items;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Modes;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Subscriptions;
using ExpiredReminderBot.Services.Transactions;

namespace ExpiredReminderBot.Commands;

public class BuySubscription : CommandBase
{
    private readonly ISubscriptionsService _subscriptionsService;
    private readonly ITransactionsService _transactionsService;
    public override string Title => "♻️ Оформить подписку";
    public override string Key => "ActivateSubscriptionCallback";

    public BuySubscription(SenderService sender, ISubscriptionsService subscriptionsService, ITransactionsService transactionsService) : base(sender)
    {
        _subscriptionsService = subscriptionsService;
        _transactionsService = transactionsService;
    }

    public override Task Execute(User user, string? data = default)
    {
        user.Mode = new ActivateSubscriptionMode(Sender, _subscriptionsService, _transactionsService);
        return user.Mode.Execute(user);
    }
}

public class ActivateSubscriptionMode : ModeBase
{
    private readonly ISubscriptionsService _subscriptionsService;
    private readonly ITransactionsService _transactionsService;

    private ActivateSubscriptionStep _step { get; set; }
    private SubscriptionPlan _selectedPlan { get; set; }

    private const string Buy = "Купить";
    private const string Cancel = "Отменить";

    public ActivateSubscriptionMode(SenderService sender, ISubscriptionsService subscriptionsService, ITransactionsService transactionsService) : base(sender)
    {
        _subscriptionsService = subscriptionsService;
        _transactionsService = transactionsService;
    }

    public override Task Execute(User user, string? data = default)
    {
        var handler = _step switch
        {
            ActivateSubscriptionStep.Init => InitStep(user),
            ActivateSubscriptionStep.SelectPlan => SelectPlanStep(user, data),
            ActivateSubscriptionStep.Confirmation => ConfirmationStep(user, data),
            _ => throw new Exception()
        };

        return handler;
    }

    public async Task InitStep(User user)
    {
        var plans = await _subscriptionsService.GetAllPlans();
        var elements = new List<InlineKeyboardElement>();

        foreach (var plan in plans)
            elements.Add(new($"{plan.Title} / {plan.Price} руб.", plan.Id));

        elements.AddRange(GetAvailableCommands(user).ToKeyboardElements());

        await SenderService.SendOrEditInlineKeyboard(user, "Выберите интересующий вас план", elements);

        _step++;
    }

    public async Task SelectPlanStep(User user, string data)
    {
        _selectedPlan = await _subscriptionsService.GetPlan(data);

        if (_selectedPlan == null)
        {
            await SenderService.SendOrEditInlineKeyboard(user, "План недоступен",
                GetAvailableCommands(user).ToKeyboardElements());
            return;
        }

        await SenderService.SendOrEditInlineKeyboard(user, $"Вы хотите купить план {_selectedPlan.Title} за {_selectedPlan.Price} руб.",
            GetAvailableOperations());

        _step++;
    }

    public async Task ConfirmationStep(User user, string data)
    {
        user.Mode = null;

        switch (data)
        {
            case Buy:
                try
                {
                    if (user.Balance - _selectedPlan.Price < 0)
                    {
                        var elements = new List<InlineKeyboardElement>()
                        {
                            new("💸 Пополнить баланс", "AddBalance", "https://t.me/eriknext")
                        };
                        elements.AddRange(GetAvailableCommands(user).ToKeyboardElements());

                        await SenderService.SendOrEditInlineKeyboard(user,
                            "Недостаточно средств на счете!", elements);

                        return;
                    }

                    var transaction = await _transactionsService.Create(user.Id, _selectedPlan.Price, TransactionType.Payment);

                    if (transaction == null)
                    {
                        await SenderService.SendOrEditInlineKeyboard(user,
                            "Не удалось произвести оплату! Обратитесь к разработчикам!", GetAvailableCommands(user).ToKeyboardElements());
                        return;
                    }

                    await _subscriptionsService.Create(user.Id, _selectedPlan.Id, DateTimeOffset.UtcNow.AddDays(_selectedPlan.Days));

                    await SenderService.SendOrEditInlineKeyboard(user,
                        "Подписка оформлена! Если подписка не появилась, обратитесь в поддержку!", GetAvailableCommands(user).ToKeyboardElements());
                }
                catch
                {
                    await SenderService.SendOrEditInlineKeyboard(user,
                        "Не удалось произвести оплату! Обратитесь к разработчикам!", GetAvailableCommands(user).ToKeyboardElements());
                }
                break;

            case Cancel:
                await SenderService.SendOrEditInlineKeyboard(user,
                    "Операция отменена!", GetAvailableCommands(user).ToKeyboardElements());
                break;
        }
    }

    public ICollection<InlineKeyboardElement> GetAvailableOperations()
    {
        return new List<InlineKeyboardElement>()
        {
            new(Buy, Buy),
            new(Cancel, Cancel)
        };
    }
}

public enum ActivateSubscriptionStep : byte
{
    Init,
    SelectPlan,
    Confirmation
}