using ExpiredReminderBot.Commands.AdminCommands;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Services;
using ExpiredReminderBot.Services.Shops;
using ExpiredReminderBot.Services.Users;
using Microsoft.AspNetCore.Components.Forms;
using User = ExpiredReminderBot.Models.User;

namespace ExpiredReminderBot.Commands;

public class MainMenuCommand : CommandBase
{
    private readonly IUsersService _usersService;
    public override string Title => "🏠 На главную";
    public override string Key => "MenuCallback";

    public MainMenuCommand(SenderService sender, IUsersService usersService) : base(sender)
    {
        _usersService = usersService;
    }

    public override async Task Execute(User user, string? data = default)
    {
        await Sender.SendOrEditInlineKeyboard(user, "Добро пожаловать! 👋🏻\nВыберите нужное действие", GetKeyboardElements(await _usersService.IsAdmin(user.Id)));
    }

    public ICollection<ICollection<InlineKeyboardElement>> GetKeyboardElements(bool admin)
    {
        var create = StorageCommands.GetCommand(typeof(CreateShopCommand));
        var shops = StorageCommands.GetCommand(typeof(MyShopsCommand));
        var profile = StorageCommands.GetCommand(typeof(MyProfileCommand));
        var adminPanel = StorageCommands.GetCommand(typeof(AdminPanelCommand));

        var elements = new List<ICollection<InlineKeyboardElement>>()
        {
            new List<InlineKeyboardElement>()
            {
                new(create.Title, create.Key),
                new(shops.Title, shops.Key)
            },
            new List<InlineKeyboardElement>()
            {
                new(profile.Title, profile.Key)
            },
            new List<InlineKeyboardElement>()
            {
                new("☎️ Поддержка", "writeToAdmins", "https://t.me/eriknext")
            },
        };

        if (admin)
            elements.Last().Add(new(adminPanel.Title, adminPanel.Key));

        return elements;
    }
}