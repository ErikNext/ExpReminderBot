using ExpiredReminderBot.Commands;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services;
using System.Reflection;

namespace ExpiredReminderBot.Modes;

public abstract class ModeBase
{
    public abstract Task Execute(User user, string? data = default);
    protected SenderService SenderService { get; }

    protected ModeBase(SenderService sender)
    {
        SenderService = sender;
    }

    public virtual IEnumerable<CommandBase> GetAvailableCommands(User user)
    {
        var commands = new List<CommandBase>()
        {
            StorageCommands.GetCommand(typeof(MainMenuCommand))
        };

        return commands;
    }
}