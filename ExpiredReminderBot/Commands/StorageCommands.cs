using System.Reflection;

namespace ExpiredReminderBot.Commands;

public static class StorageCommands
{
    private static Dictionary<string, CommandBase> _commands = new();

    public static void Init(IServiceProvider serviceProvider)
    {
        var allTypes = Assembly.GetAssembly(typeof(CommandBase)).GetTypes();
        foreach (Type type in
                     allTypes
                     .Where(myType => myType.IsClass 
                     && !myType.IsAbstract 
                     && myType.IsSubclassOf(typeof(CommandBase))))
        {
            var command = (CommandBase) serviceProvider.GetService(type);
            _commands.Add(command.Key, command);
        }
    }

    public static Dictionary<string, CommandBase> GetCommands()
    {
        return _commands;
    }

    public static CommandBase GetCommand(Type type)
    {
        var command = _commands.Values.FirstOrDefault(x => x.GetType() == type);

        if (command == default)
        {
            throw new InvalidOperationException();
        }

        return command;
    }
}