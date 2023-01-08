using ExpiredReminderBot.Commands;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Services.Core;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = ExpiredReminderBot.Models.User;

namespace ExpiredReminderBot.Services;

public class SenderService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;

    public SenderService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task<Message> SendMessage(long userId, string message)
    {
        _logger.LogInformation($"Send message: '{message}' to user with chat id: {userId}");

        var sentMessage = await _botClient.SendTextMessageAsync(
            chatId: userId,
            text: message);

        return sentMessage;
    }

    public async Task SendInlineKeyboard(
        User user,
        string message,
        ICollection<InlineKeyboardElement> keyboardElements,
        ParseMode parseMode = ParseMode.Html)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        if (keyboardElements != null)
        {
            foreach (var item in keyboardElements)
            {
                var button = InlineKeyboardButton.WithCallbackData(item.Text, item.CallbackData);
                button.Url = item.Url;
                buttons.Add(new[] { button });
            }
        }

        var sentMessage = await _botClient.SendTextMessageAsync(chatId: user.TelegramId,
            text: message,
            replyMarkup: new InlineKeyboardMarkup(buttons),
            parseMode: parseMode);

        user.LastReceivedMessage = sentMessage;
    }

    public async Task RemoveMessage(User user, int messageId)
    {
        await _botClient.DeleteMessageAsync(user.TelegramId, messageId);
    }

    public async Task SendOrEditInlineKeyboard(
        User user,
        string message,
        ICollection<InlineKeyboardElement> keyboardElements,
        ParseMode parseMode = ParseMode.Html)
    {
        if (user.LastReceivedMessage == null || user.LastReceivedMessage.Text == message)
        {
            await SendInlineKeyboard(user, message, keyboardElements, parseMode);
            return;
        }

        var buttons = new List<InlineKeyboardButton[]>();

        if (keyboardElements != null)
        {
            foreach (var item in keyboardElements)
            {
                var button = InlineKeyboardButton.WithCallbackData(item.Text, item.CallbackData);
                button.Url = item.Url;
                buttons.Add(new[] { button });
            }
        }

        var sentMessage = await _botClient.EditMessageTextAsync(chatId: user.TelegramId,
            messageId: user.LastReceivedMessage.MessageId,
            text: message,
            replyMarkup: new InlineKeyboardMarkup(buttons),
            parseMode: parseMode);

        user.LastReceivedMessage = sentMessage;
    }

    public Task SendSubOffer(User user)
    {
        var elements = new List<InlineKeyboardElement>();

        var buyCommand = StorageCommands.GetCommand(typeof(BuySubscription));
        var menuCommand = StorageCommands.GetCommand(typeof(MainMenuCommand));

        elements.Add(new(buyCommand.Title, buyCommand.Key));
        elements.Add(new(menuCommand.Title, menuCommand.Key));

        return SendOrEditInlineKeyboard(user,
            "Команда недоступна ❗️\nНеобходимо оформить подписку 💸",
            elements);
    }

    public async Task SendOrEditInlineKeyboard(
        User user,
        string message,
        ICollection<ICollection<InlineKeyboardElement>> keyboardElements,
        ParseMode parseMode = ParseMode.Html)
    {
        var buttons = new List<List<InlineKeyboardButton>>();

        if (keyboardElements != null)
        {
            foreach (var line in keyboardElements)
            {
                var elements = new List<InlineKeyboardButton>();
                foreach (var element in line)
                {
                    var button = InlineKeyboardButton.WithCallbackData(element.Text, element.CallbackData);
                    button.Url = element.Url;
                    elements.Add(button);
                }
                buttons.Add(elements);
            }
        }

        if (user.LastReceivedMessage == null || user.LastReceivedMessage.Text == message)
        {
            user.LastReceivedMessage = await _botClient.SendTextMessageAsync(chatId: user.TelegramId,
                text: message,
                replyMarkup: new InlineKeyboardMarkup(buttons),
                parseMode: parseMode);

            return;
        }

        var sentMessage = await _botClient.EditMessageTextAsync(chatId: user.TelegramId,
            messageId: user.LastReceivedMessage.MessageId,
            text: message,
            replyMarkup: new InlineKeyboardMarkup(buttons),
            parseMode: parseMode);

        user.LastReceivedMessage = sentMessage;
    }

    public async Task EditMessage(
        long chatId,
        int messageId,
        string message)
    {
        await _botClient.EditMessageTextAsync(chatId: chatId,
            messageId: messageId,
            text: message);
    }
}