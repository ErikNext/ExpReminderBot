using ExpiredReminderBot.Database.Rows;
using Microsoft.EntityFrameworkCore;

namespace ExpiredReminderBot.Database;

public class ExpiredReminderDbContext : DbContext
{
    public DbSet<UserRow> Users { get; set; } = null!;
    public DbSet<ProductRow> Products { get; set; } = null!;
    public DbSet<ShopRow> Shops { get; set; } = null!;
    public DbSet<ShopAccessRow> ShopsAccess { get; set; } = null!;
    public DbSet<TransactionRow> Transactions { get; set; } = null!;
    public DbSet<SubscriptionRow> Subscriptions { get; set; } = null!;
    public DbSet<AdminRow> Admins { get; set; } = null!;
    public DbSet<SubscriptionPlanRow> SubscriptionPlans { get; set; } = null!;

    public ExpiredReminderDbContext(DbContextOptions<ExpiredReminderDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        BuildUserRow(modelBuilder);
        BuildShopRow(modelBuilder);
        BuildProductRow(modelBuilder);
        BuildShopAccess(modelBuilder);
        BuildTransactionRow(modelBuilder);
        BuildSubscriptionRow(modelBuilder);
        BuildAdminRow(modelBuilder);
        BuildSubscriptionPlanRow(modelBuilder);
    }

    private static void BuildSubscriptionPlanRow(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<SubscriptionPlanRow>()
            .HasIndex(u => u.Id)
            .IsUnique();
        modelBuilder
            .Entity<SubscriptionPlanRow>()
            .Property(u => u.Title);
        modelBuilder
            .Entity<SubscriptionPlanRow>()
            .Property(u => u.Days);
        modelBuilder
            .Entity<SubscriptionPlanRow>()
            .Property(u => u.Price);

        modelBuilder
            .Entity<SubscriptionPlanRow>()
            .HasMany(x => x.Subscriptions)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.PlanId);
    }

    private static void BuildAdminRow(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<AdminRow>()
            .HasIndex(u => u.Id)
            .IsUnique();
        modelBuilder
            .Entity<AdminRow>()
            .Property(u => u.UserId);
    }

    private static void BuildUserRow(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<UserRow>()
            .HasIndex(u => u.Id)
            .IsUnique();
        modelBuilder
            .Entity<UserRow>()
            .Property(u => u.Username);
        modelBuilder
            .Entity<UserRow>()
            .Property(u => u.Balance);
        modelBuilder
            .Entity<UserRow>()
            .Property(u => u.TelegramId);
        modelBuilder
            .Entity<UserRow>()
            .Property(u => u.Subscription);
        modelBuilder
            .Entity<UserRow>()
            .Property(u => u.CreatedDate);
    }

    private static void BuildShopRow(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ShopRow>()
            .HasIndex(u => u.Id)
            .IsUnique();
        modelBuilder
            .Entity<ShopRow>()
            .Property(u => u.Title);
        modelBuilder
            .Entity<ShopRow>()
            .Property(u => u.DirectorId);

        modelBuilder
            .Entity<ShopRow>()
            .HasMany(x => x.Products)
            .WithOne(x => x.Shop)
            .HasForeignKey(x => x.ShopId);
    }

    private static void BuildProductRow(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ProductRow>()
            .HasIndex(u => u.Id)
            .IsUnique();
        modelBuilder
            .Entity<ProductRow>()
            .Property(u => u.Title);
        modelBuilder
            .Entity<ProductRow>()
            .Property(u => u.GoldCode);
        modelBuilder
            .Entity<ProductRow>()
            .Property(u => u.ShopId);
        modelBuilder
            .Entity<ProductRow>()
            .Property(u => u.ExpiryDate);
        modelBuilder
            .Entity<ProductRow>()
            .Property(u => u.CreatedDate);
    }

    private static void BuildShopAccess(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ShopAccessRow>()
            .HasIndex(u => u.Id)
            .IsUnique();
        modelBuilder
            .Entity<ShopAccessRow>()
            .HasIndex(u => u.ShopId);
        modelBuilder
            .Entity<ShopAccessRow>()
            .HasIndex(u => u.UserId);

        modelBuilder
            .Entity<ShopAccessRow>()
            .HasOne(x => x.User)
            .WithMany(x => x.ShopAccessRows)
            .HasForeignKey(x => x.UserId);

    }

    private static void BuildTransactionRow(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<TransactionRow>()
            .HasIndex(u => u.Id)
            .IsUnique();
        modelBuilder
            .Entity<TransactionRow>()
            .Property(u => u.UserId);
        modelBuilder
            .Entity<TransactionRow>()
            .Property(u => u.Amount);
        modelBuilder
            .Entity<TransactionRow>()
            .Property(u => u.Type)
            .HasConversion<string>();
        modelBuilder
            .Entity<TransactionRow>()
            .Property(u => u.CreatedDate);
    }

    private static void BuildSubscriptionRow(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<SubscriptionRow>()
            .HasIndex(u => u.Id)
            .IsUnique();
        modelBuilder
            .Entity<SubscriptionRow>()
            .Property(u => u.UserId);
        modelBuilder
            .Entity<SubscriptionRow>()
            .Property(u => u.EndDate);
        modelBuilder
            .Entity<SubscriptionRow>()
            .Property(u => u.CreatedDate);
    }
}