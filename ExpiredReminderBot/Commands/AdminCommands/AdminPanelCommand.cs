using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Users;

namespace ExpiredReminderBot.Commands.AdminCommands;

public class AdminPanelCommand : CommandBase
{
    private readonly IUsersService _usersService;
    public override string Title => "⚙️ Админ панель";
    public override string Key => "AdminPanelCallback";

    public AdminPanelCommand(SenderService sender, IUsersService usersService) : base(sender)
    {
        _usersService = usersService;
    }

    public override async Task Execute(User user, string? data = default)
    {
        if (!await _usersService.IsAdmin(user.Id))
            return;

        await Sender.SendOrEditInlineKeyboard(user, "Доступные команды:",
            GetAvailableCommands(user).ToKeyboardElements());
    }

    public override IEnumerable<CommandBase> GetAvailableCommands(User user)
    {
        var commands = new List<CommandBase>()
        {
            StorageCommands.GetCommand(typeof(ReplenishmentBalanceCommand)),
            StorageCommands.GetCommand(typeof(MainMenuCommand))
        };

        return commands;
    }


}