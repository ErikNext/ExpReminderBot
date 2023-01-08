using ExpiredReminderBot.Commands;

namespace ExpiredReminderBot.Services.Core
{
    public class InitHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public InitHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StorageCommands.Init(_serviceProvider);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
