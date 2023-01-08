using ExpiredReminderBot.Services;
using Telegram.Bot.Types;
using User = ExpiredReminderBot.Models.User;

namespace ExpiredReminderBot.Commands;

public abstract class CommandBase
{
    public abstract string Title { get; }
    public abstract string Key { get; }

    protected SenderService Sender { get; }

    protected CommandBase(SenderService sender)
    {
        Sender = sender;
    }

    public abstract Task Execute(User user, string? data = default);

    public virtual IEnumerable<CommandBase> GetAvailableCommands(User user)
    {
        var commands = new List<CommandBase>()
        {
            StorageCommands.GetCommand(typeof(MainMenuCommand))
        };

        return commands;
    }
}