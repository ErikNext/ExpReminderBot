using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using ExpiredReminderBot.Database;
using ExpiredReminderBot.Database.Rows;
using ExpiredReminderBot.Extensions;
using ExpiredReminderBot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpiredReminderBot.Services.Users;

public class UsersService : IUsersService
{
    private readonly ExpiredReminderDbContext _dbContext;
    private static ConcurrentDictionary<string, User> _cacheUsers { get; } = new();

    public UsersService(ExpiredReminderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> Create(long telegramId, string username)
    {
        var row = new UserRow(
            Ulid.NewUlid().ToString(),
            username,
            0,
            telegramId,
            DateTimeOffset.UtcNow);

        await _dbContext.Users
            .AddAsync(row)
            .ConfigureAwait(false);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);

        return GetOrCreateUserOfCache(row);
    }

    public async Task<User> GetOrCreate(long telegramId, string username)
    {
        var row = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.TelegramId == telegramId)
            .ConfigureAwait(false);

        if (row == null)
        {
            return await Create(telegramId, username);
        }

        return GetOrCreateUserOfCache(row);
    }

    public async Task<User?> Get(string id)
    {
        var row = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id)
            .ConfigureAwait(false);

        if (row == null)
            return null;


        return MapToDto(row);
    }

    public async Task<bool> IsAdmin(string userId)
    {
        var row = await _dbContext.Admins.FirstOrDefaultAsync(x => x.UserId == userId);

        if (row == null)
            return false;

        return true;
    }

    public async Task<User> Update(string userId, UpdateUserRequest request)
    {
        var row = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (row == null)
        {
            throw new ArgumentNullException($"Can't find user for update: {userId}");
        }

        row.Balance = request.Balance ?? row.Balance;
        row.Subscription = request.Subscription ?? row.Subscription;

        _dbContext.Users.Update(row);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);

        return UpdateOfCache(row);
    }

    private static User UpdateOfCache(UserRow row)
    {
        var user = GetOrCreateUserOfCache(row);

        user.Balance = row.Balance;
        user.Subscription = row.Subscription;

        return _cacheUsers.AddOrUpdate(user.Id, user);
    }

    private static User GetOrCreateUserOfCache(UserRow row)
    {
        _cacheUsers.TryGetValue(row.Id, out var userOfCache);

        if (userOfCache == null)
        {
            var user = MapToDto(row);
            _cacheUsers.TryAdd(row.Id, user);
            return user;
        }

        return userOfCache;
    }

    private static User MapToDto(UserRow row)
    {
        return new User(row.Id, row.TelegramId, row.Username, row.Balance, row.Subscription);
    }
}

public record UpdateUserRequest(decimal? Balance = null, bool? Subscription = null);