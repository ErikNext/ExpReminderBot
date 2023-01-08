using ExpiredReminderBot.Models;

namespace ExpiredReminderBot.Services.Shops;

public interface IShopsService
{
    Task Create(CreateShopRequest request);

    Task<ICollection<Shop>> GetUserShops(string userId);

    Task<Shop> Get(string shopId);

    Task Delete(string userId, string shopId);

    Task<bool> TryAttachToShop(string directorId, string managerId, string shopId);

    Task<ICollection<User>> GetShopEmployees(string shopId);

    Task<User?> TryUnpinEmployee(string shopId, string userId);
}

