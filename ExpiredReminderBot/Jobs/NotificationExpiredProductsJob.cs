using ExpiredReminderBot.Commands;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Products;
using ExpiredReminderBot.Services.Shops;
using ExpiredReminderBot.Services.Users;
using Quartz;

namespace ExpiredReminderBot.Jobs;

[DisallowConcurrentExecution]
public class NotificationExpiredProductsJob : IJob
{
    private readonly SenderService _senderService;
    private readonly IProductsService _productsService;
    private readonly IShopsService _shopsService;
    private readonly IUsersService _usersService;

    private readonly CommandBase _deleteProductCommand;
    private readonly List<InlineKeyboardElement> _elements;

    public NotificationExpiredProductsJob(
        SenderService senderService, 
        IProductsService productsService, 
        IShopsService shopsService, 
        IUsersService usersService)
    {
        _senderService = senderService;
        _productsService = productsService;
        _shopsService = shopsService;
        _usersService = usersService;

        _deleteProductCommand = StorageCommands.GetCommand(typeof(DeleteProductCommand));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var products = await _productsService.SearchExpiredProducts(3);

        foreach (var product in products)
        {
            var shop = await _shopsService.Get(product.ShopId);
            var user = await _usersService.Get(shop.DirectorId);

            if(shop is null || user is null)
                continue;

            var message = $"Внимание! Магазин: {shop.Title}💡" +
                          $"\nТовар: {product.Title}" +
                          $"\nGold: {product.GoldCode}" +
                          $"\nВыйдет из срока: {product.ExpiryDate.Date.ToShortDateString()}";


            var elements = new List<InlineKeyboardElement>()
            {
                new(_deleteProductCommand.Title, $"{_deleteProductCommand.Key}:{product.Id}")
            };

            await _senderService.SendInlineKeyboard(user, message, elements);
        }
    }
}