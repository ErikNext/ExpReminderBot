using ExpiredReminderBot.Database;
using ExpiredReminderBot.Database.Rows;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services.Users;
using Microsoft.EntityFrameworkCore;
using NUlid;

namespace ExpiredReminderBot.Services.Shops;

public class ShopsService : IShopsService
{
    private readonly ExpiredReminderDbContext _dbContext;
    private readonly IUsersService _usersService;

    public ShopsService(ExpiredReminderDbContext dbContext, IUsersService usersService)
    {
        _dbContext = dbContext;
        _usersService = usersService;
    }

    public async Task Create(CreateShopRequest request)
    {
        var row = new ShopRow(
            Ulid.NewUlid().ToString(),
            request.Title,
            request.UserId);

        await _dbContext.Shops
            .AddAsync(row)
            .ConfigureAwait(false);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);
    }

    public async Task<ICollection<Shop>> GetUserShops(string userId)
    {
        var rows = await _dbContext.Shops.Where(x => x.DirectorId == userId)
            .AsNoTracking()
            .ToListAsync();

        var shopAccessRows = await _dbContext.ShopsAccess.Where(x => x.UserId == userId).ToListAsync();

        foreach (var item in shopAccessRows)
        {
            var row = await _dbContext.Shops.FirstOrDefaultAsync(x => x.Id == item.ShopId);

            if(row == null)
                continue;

            rows.Add(row);
        }

        return rows.Select(MapToDto).ToList();
    }

    public async Task<Shop> Get(string shopId)
    {
        var row = await _dbContext.Shops.FirstOrDefaultAsync(x => x.Id == shopId);

        if (row == null)
            return null;

        return MapToDto(row);
    }

    public async Task Delete(string userId, string shopId)
    {
        var row = await _dbContext.Shops.FirstOrDefaultAsync(x => x.DirectorId == userId && x.Id == shopId);

        if (row == null)
        {
            throw new ArgumentException($"Can't find this shop {shopId} or user dont have permisions");
        }

        _dbContext.Shops.Remove(row);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> TryAttachToShop(string directorId, string managerId, string shopId)
    {
        var shop = await Get(shopId);

        if (shop == null)
        {
            return false;
        }

        var row = new ShopAccessRow(Ulid.NewUlid().ToString(), shopId, managerId);

        await _dbContext.ShopsAccess
            .AddAsync(row)
            .ConfigureAwait(false);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);

        return true;
    }

    public async Task<ICollection<User>> GetShopEmployees(string shopId)
    {
        var shopAccess = await _dbContext.ShopsAccess.Where(x => x.ShopId == shopId).ToListAsync();
        var employees = new List<User>();

        foreach (var access in shopAccess)
        {
            var employee = await _usersService.Get(access.UserId);

            if(employee == null)
                continue;

            employees.Add(employee);
        }

        return employees;
    }

    public async Task<User?> TryUnpinEmployee(string shopId, string userId)
    {
        var user = await _usersService.Get(userId);
        var rowAccess = await _dbContext.ShopsAccess.FirstOrDefaultAsync(x => x.UserId == userId && x.ShopId == shopId);

        if (rowAccess == null || user == null)
        {
            return null;
        }

        _dbContext.ShopsAccess.Remove(rowAccess);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);

        return user;
    }

    public static Shop MapToDto(ShopRow row)
    {
        return new Shop(row.Id, row.Title, row.DirectorId);
    }
}