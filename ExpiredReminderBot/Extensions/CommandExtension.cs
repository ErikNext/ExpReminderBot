using ExpiredReminderBot.Commands;

namespace ExpiredReminderBot.Extensions;

public static class CommandExtension
{
    public static ICollection<InlineKeyboardElement> ToKeyboardElements(this IEnumerable<CommandBase> commands)
    {
        List<InlineKeyboardElement> elements = new();

        foreach (var command in commands)
        {
            elements.Add(new(command.Title, command.Key));
        }

        return elements;
    }
}