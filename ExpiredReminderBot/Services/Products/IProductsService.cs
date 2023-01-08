using ExpiredReminderBot.Models;

namespace ExpiredReminderBot.Services.Products;

public interface IProductsService
{
    Task<Product> Create(CreateProductRequest request, string shopId);

    Task<ICollection<Product>> SearchExpiredProducts(int days);

    Task<Product?> Delete(string id);
}