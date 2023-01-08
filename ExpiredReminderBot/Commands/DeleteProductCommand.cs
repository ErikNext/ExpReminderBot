using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Products;

namespace ExpiredReminderBot.Commands;

public class DeleteProductCommand : CommandBase
{
    private readonly IProductsService _productsService;
    public override string Title => "Удалить";
    public override string Key => "DeleteProductCallback";

    public DeleteProductCommand(SenderService sender, IProductsService productsService) : base(sender)
    {
        _productsService = productsService;
    }


    public override async Task Execute(User user, string? data = default)
    {
        var product = await _productsService.Delete(data);

        if (product == null)
        {
            await Sender.SendOrEditInlineKeyboard(user, "Ошибка удаления продукта! Обратитесь к разработчикам!",
                GetAvailableCommands(user).ToKeyboardElements());
            return;
        }

        await Sender.RemoveMessage(user, user.LastSendMessage.MessageId);
        await Sender.SendOrEditInlineKeyboard(user, $"Продукт '{product.Title}' был удален!",
            GetAvailableCommands(user).ToKeyboardElements());
    }
}