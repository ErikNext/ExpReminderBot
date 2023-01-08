using ExpiredReminderBot.Database;
using ExpiredReminderBot.Database.Items;
using ExpiredReminderBot.Database.Rows;
using ExpiredReminderBot.Models;
using ExpiredReminderBot.Services.Users;
using Microsoft.EntityFrameworkCore;

namespace ExpiredReminderBot.Services.Transactions;

public class TransactionsService : ITransactionsService
{
    private readonly ExpiredReminderDbContext _dbContext;
    private readonly IUsersService _usersService;

    public TransactionsService(ExpiredReminderDbContext dbContext, IUsersService usersService)
    {
        _dbContext = dbContext;
        _usersService = usersService;
    }

    public async Task<Transaction> Create(string userId, decimal amount, TransactionType type)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            throw new InvalidOperationException("Operation: Create transaction\nException: Can't find user");
        }

        decimal newBalance = user.Balance;

        if (type == TransactionType.Replenishment) 
            newBalance += amount;
        else if (type == TransactionType.Payment) 
            newBalance -= amount;
        else
            throw new InvalidOperationException("Operation: Create transaction\nException: Can't find TransactionType");

        var transaction = new TransactionRow(Ulid.NewUlid().ToString(), userId, amount, type, DateTimeOffset.UtcNow);

        await _usersService.Update(user.Id, new(newBalance));

        await _dbContext.Transactions
            .AddAsync(transaction)
            .ConfigureAwait(false);

        await _dbContext
            .SaveChangesAsync()
            .ConfigureAwait(false);

        return MapToDto(transaction);
    }

    private static Transaction MapToDto(TransactionRow row)
    {
        return new Transaction(row.Id, row.UserId, row.Amount, row.Type, row.CreatedDate);
    }
}