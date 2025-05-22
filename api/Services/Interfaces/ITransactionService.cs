using api.Models;

namespace api.Services.Interfaces
{
    public interface ITransactionService
    {
        Transaction CreateTransaction(TransactionType transactionType, decimal amount, Wallet fromWallet, Wallet? toWallet);
        Task<List<Transaction>> GetTransactions(Wallet wallet, DateTime? startDate, DateTime? endDate);
    }
}
