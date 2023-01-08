using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Services;
using ExpiredReminderBot;
using ExpiredReminderBot.Database;
using ExpiredReminderBot.Jobs;
using Telegram.Bot;
using ExpiredReminderBot.Services.Core;
using ExpiredReminderBot.Services.Products;
using ExpiredReminderBot.Services.Shops;
using ExpiredReminderBot.Services.Subscriptions;
using ExpiredReminderBot.Services.Transactions;
using ExpiredReminderBot.Services.Users;
using Quartz;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var botConfig = builder.Configuration.GetSection("BotConfiguration").Get<BotConfiguration>();


builder.Services.AddHostedService<ConfigureWebhook>();
builder.Services.AddHttpClient("tgwebhook")
    .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(botConfig.BotToken, httpClient));

builder.Services.AddScoped<HandleUpdateService>();
builder.Services.AddHostedService<InitHostedService>();

builder.Services.AddSingleton<SenderService>();
builder.Services.AddSingleton<IUsersService, UsersService>();
builder.Services.AddSingleton<IShopsService, ShopsService>();
builder.Services.AddSingleton<IProductsService, ProductsService>();
builder.Services.AddSingleton<ITransactionsService, TransactionsService>();
builder.Services.AddSingleton<ISubscriptionsService, SubscriptionsService>();

builder.Services.AddAllCommandsAsSingleton();
builder.Services.AddMvc();
builder.Services.InitializeContext(builder.Configuration);

builder.Services.AddQuartz(
    q => 
    { 
        q.UseMicrosoftDependencyInjectionJobFactory(); 
        q.AddJob<NotificationExpiredProductsJob>(opts => opts.WithIdentity(nameof(NotificationExpiredProductsJob)));
        q.AddTrigger(
            opts => opts
                .ForJob(nameof(NotificationExpiredProductsJob))
                .WithIdentity(nameof(NotificationExpiredProductsJob) + "-trigger")
                .WithCronSchedule("0 0 12 1/1 * ? *"));
                //.WithSimpleSchedule(
                //    x => x
                //        .WithIntervalInMinutes(30)
                //        .RepeatForever()));
        q.UseMicrosoftDependencyInjectionJobFactory();
        q.AddJob<DeleteExpiredSubscriptionsJob>(opts => opts.WithIdentity(nameof(DeleteExpiredSubscriptionsJob)));
        q.AddTrigger(
            opts => opts
                .ForJob(nameof(DeleteExpiredSubscriptionsJob))
                .WithIdentity(nameof(DeleteExpiredSubscriptionsJob) + "-trigger")
                .WithCronSchedule("0 59 23 1/1 * ? *"));
                //.WithSimpleSchedule(
                //    x => x
                //        .WithIntervalInMinutes(30)
                //        .RepeatForever()));
    });

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

builder.Services.AddControllers().AddNewtonsoftJson();
var app = builder.Build();

app.UseRouting();
app.UseCors();

app.UseEndpoints(endpoints =>
{
    var token = botConfig.BotToken;
    endpoints.MapControllerRoute(name: "tgwebhook",
        pattern: $"bot/{token}",
        new { controller = "Webhook", action = "Post" });
    endpoints.MapControllers();
});

app.Run();