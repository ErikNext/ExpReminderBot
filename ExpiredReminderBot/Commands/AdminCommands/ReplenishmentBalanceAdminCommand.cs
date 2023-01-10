using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Modes;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Transactions;
using ExpiredReminderBot.Services.Users;
using ExpiredReminderBot.Database.Items;

namespace ExpiredReminderBot.Commands.AdminCommands;

public class ReplenishmentBalanceCommand : CommandBase
{
    private readonly IUsersService _usersService;
    private readonly ITransactionsService _transactionsService;
    public override string Title => "💸 Пополнить баланс";
    public override string Key => "ReplenishmentBalanceCallback";

    public ReplenishmentBalanceCommand(SenderService sender, IUsersService usersService, ITransactionsService transactionsService) : base(sender)
    {
        _usersService = usersService;
        _transactionsService = transactionsService;
    }


    public override async Task Execute(User user, string? data = default)
    {
        if (!await _usersService.IsAdmin(user.Id))
            return;

        user.Mode = new ReplenishmentBalanceMode(Sender, _transactionsService, _usersService);
        await user.Mode.Execute(user);
    }
}

public class ReplenishmentBalanceMode : ModeBase
{
    private readonly ITransactionsService _transactionsService;
    private readonly IUsersService _usersService;
    public ReplenishmentBalanceStep _step;

    private User? _userForReplenishment;
    private decimal _amountForReplenishment;

    private const string Confirm = "Пополнить";
    private const string Cancel = "Отменить";

    public ReplenishmentBalanceMode(SenderService sender, ITransactionsService transactionsService, IUsersService usersService) : base(sender)
    {
        _transactionsService = transactionsService;
        _usersService = usersService;
    }

    public override Task Execute(User user, string? data = default)
    {
        var handler = _step switch
        {
            ReplenishmentBalanceStep.Init => InitialStep(user),
            ReplenishmentBalanceStep.SetUserId => SetUserIdStep(user, data),
            ReplenishmentBalanceStep.SetAmount => SetAmount(user, data),
            ReplenishmentBalanceStep.Confirmation => ConfirmationStep(user, data),
            _ => throw new Exception()
        };

        return handler;
    }

    public async Task InitialStep(User user)
    {
        await SenderService.SendOrEditInlineKeyboard(user, "Отправьте ID пользователя для пополнения баланса",
            GetAvailableCommands(user).ToKeyboardElements());

        _step++;
    }

    public async Task SetUserIdStep(User user, string data)
    {
        _userForReplenishment = await _usersService.Get(data);

        if(_userForReplenishment == null)
        {
            if(long.TryParse(data, out var tgId))
            {
                _userForReplenishment = await _usersService.GetByTelegramId(tgId);
            }
        }

        if (_userForReplenishment == null)
        {
            await SenderService.SendInlineKeyboard(user, "Пользователь не найден! Попробуйте еще раз.",
                GetAvailableCommands(user).ToKeyboardElements());
            return;
        }

        await SenderService.SendMessage(user.TelegramId,
            $"Пользователь найден!\nUsername: {_userForReplenishment.Username}\nБаланс: {_userForReplenishment.Balance} руб.");

        await SenderService.SendInlineKeyboard(user, "Отправьте сумму пополнения",
            GetAvailableCommands(user).ToKeyboardElements());

        _step++;
    }

    public async Task SetAmount(User user, string data)
    {
        if (!decimal.TryParse(data, out var amount))
        {
            await SenderService.SendInlineKeyboard(user, "Ошибка! Проверьте правильность ввода!",
                GetAvailableCommands(user).ToKeyboardElements());
            return;
        }

        if (_userForReplenishment == null)
        {
            await SenderService.SendInlineKeyboard(user, "Пользователь не найден!",
                GetAvailableCommands(user).ToKeyboardElements());
            return;
        }

        _amountForReplenishment = amount;

        await SenderService.SendInlineKeyboard(user, $"Пользователь: {_userForReplenishment.Username}" +
                                                            $"\nБаланс: {_userForReplenishment.Balance} руб." +
                                                            $"\nСумма пополнения: {_amountForReplenishment} руб.",
            GetAvailableOperations());

        _step++;
    }

    public async Task ConfirmationStep(User user, string data)
    {
        user.Mode = null;

        if (!await _usersService.IsAdmin(user.Id))
        {
            await SenderService.SendInlineKeyboard(user, "Недостаточно прав!",
                GetAvailableCommands(user).ToKeyboardElements());
            return;
        }

        if (_userForReplenishment == null)
        {
            await SenderService.SendInlineKeyboard(user, "Пользователь не найден!",
                GetAvailableCommands(user).ToKeyboardElements());
            return;
        }

        switch (data)
        {
            case Confirm:
                try
                {
                    var transaction = await _transactionsService.Create(_userForReplenishment.Id, _amountForReplenishment,
                        TransactionType.Replenishment);

                    await SenderService.SendOrEditInlineKeyboard(user,
                        $"Пользователю {_userForReplenishment.Username} начислено {transaction.Amount} руб!", GetAvailableCommands(user).ToKeyboardElements());
                }
                catch
                {
                    await SenderService.SendOrEditInlineKeyboard(user,
                        "Не удалось удалить магазин! Обратиесь к разработчикам!", GetAvailableCommands(user).ToKeyboardElements());
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
            new(Confirm, Confirm),
            new(Cancel, Cancel)
        };
    }
}

public enum ReplenishmentBalanceStep : byte
{
    Init,
    SetUserId,
    SetAmount,
    Confirmation
}