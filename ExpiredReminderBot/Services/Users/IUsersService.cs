using ExpiredReminderBot.Models;
using Microsoft.AspNetCore.Components.Web;

namespace ExpiredReminderBot.Services.Users;

public interface IUsersService
{
    Task<User> Create(long telegramId, string username);

    Task<User> GetOrCreate(long telegramId, string username);

    Task<User?> Get(string id);

    Task<User?> GetByTelegramId(long id);

    Task<bool> IsAdmin(string userId);

    Task<User> Update(string userId, UpdateUserRequest request);
}