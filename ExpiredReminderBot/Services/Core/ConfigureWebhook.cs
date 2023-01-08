using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ExpiredReminderBot.Services.Core;

public class ConfigureWebhook : IHostedService
{
    private readonly ILogger<ConfigureWebhook> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly BotConfiguration _configuration;

    public ConfigureWebhook(
        ILogger<ConfigureWebhook> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        var webhookAddress = @$"{_configuration.HostAddress}/bot/{_configuration.BotToken}";

        _logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);


        await botClient.SetWebhookAsync(
            url: webhookAddress,
            allowedUpdates: Array.Empty<UpdateType>(),
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        _logger.LogInformation("Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}