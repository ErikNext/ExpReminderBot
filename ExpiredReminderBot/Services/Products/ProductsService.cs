using ExpiredReminderBot.Database;
using ExpiredReminderBot.Database.Rows;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services.Shops;
using Microsoft.EntityFrameworkCore;

namespace ExpiredReminderBot.Services.Products;

public class ProductsService : IProductsService
{
    private readonly ExpiredReminderDbContext _dbContext;

    public ProductsService(ExpiredReminderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product> Create(CreateProductRequest request, string shopId)
    {
        var row = new ProductRow(
            Ulid.NewUlid().ToString(),
            request.Title,
            request.GoldCode,
            shopId,
            request.ExpiryDate.UtcDateTime,
            DateTimeOffset.UtcNow);

        await _dbContext.Products
            .AddAsync(row)
            .ConfigureAwait(false);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);
       
        return MapToDto(row);
    }

    public async Task<ICollection<Product>> SearchExpiredProducts(int days)
    {
        var rows = await _dbContext.Products.Where(x => x.ExpiryDate <= DateTimeOffset.UtcNow.AddDays(days))
            .ToListAsync();

        return rows.Select(MapToDto).ToList();
    }

    public async Task<Product?> Delete(string id)
    {
        var row = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

        if (row == null)
            return null;

        _dbContext.Products.Remove(row);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);

        return MapToDto(row);
    }

    public static Product MapToDto(ProductRow row)
    {
        return new Product(
            row.Id,
            row.Title,
            row.GoldCode,
            row.ShopId, 
            row.ExpiryDate,
            row.CreatedDate);
    }
}