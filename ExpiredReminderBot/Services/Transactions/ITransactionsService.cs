using ExpiredReminderBot.Database.Items;
using ExpiredReminderBot.Models;

namespace ExpiredReminderBot.Services.Transactions;

public interface ITransactionsService
{
    Task<Transaction> Create(
        string userId,
        decimal amount, 
        TransactionType type);
}