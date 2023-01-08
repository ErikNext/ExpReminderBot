using ExpiredReminderBot.Commands;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Services.Users;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ExpiredReminderBot.Services.Core;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly IUsersService _usersService;
    private readonly SenderService _sender;

    public HandleUpdateService(
        ITelegramBotClient botClient,
        ILogger<HandleUpdateService> logger,
        IUsersService usersService,
        SenderService sender)
    {
        _botClient = botClient;
        _logger = logger;
        _usersService = usersService;
        _sender = sender;
    }

    public async Task EchoAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(update.Message!),
            UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!),
            _ => UnknownUpdateHandlerAsync(update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    private async Task BotOnMessageReceived(Message message)
    {
        var user = await _usersService.GetOrCreate(message.From.Id, message.From.Username ?? message.From.FirstName);
        user.LastSendMessage = message;
        await ProcessUserRequest(user, message.Text);
    }

    private async Task ProcessUserRequest(Models.User user, string data)
    {
        var commandAndData = data.ToKeyValuePairs();

        if (StorageCommands.GetCommands().TryGetValue(commandAndData.Key, out var command))
        {
            _logger.LogInformation($"User: '@{user.Username}' execute command: '{command.Title}'");
            await command.Execute(user, commandAndData.Value);
        }
        else if (user.Mode != null)
        {
            await user.Mode.Execute(user, data); 
            return;
        }
        else
            await StorageCommands.GetCommand(typeof(MainMenuCommand)).Execute(user);
    }

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
    {
        var user = await _usersService.GetOrCreate(callbackQuery.From.Id, callbackQuery.From.Username ?? callbackQuery.From.FirstName);
        user.LastSendMessage = callbackQuery.Message;
        await ProcessUserRequest(user, callbackQuery.Data);
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);

        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(Exception exception)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
        return Task.CompletedTask;
    }
}
