using api.Data;
using api.Models;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace api.Services
{
    public class TransactionService(AppDbContext context) : ITransactionService
    {
        public readonly AppDbContext _context = context;

        public Transaction CreateTransaction(TransactionType transactionType, decimal amount, Wallet fromWallet, Wallet? toWallet)
        {
            var transaction = new Transaction()
            {
                Amount = amount,
                FromWalletId = fromWallet.Id,
                ToWalletId = toWallet?.Id,
                CreatedAt = DateTime.UtcNow,
                TransactionType = transactionType,
                Status = TransactionStatus.COMPLETED
            };

            return transaction;
        }

        public async Task<List<Transaction>> GetTransactions(Wallet wallet, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Transactions
                .Include(x => x.FromWallet)
                .Include(x => x.ToWallet)
                .Where(t => t.FromWalletId == wallet.Id || t.ToWalletId == wallet.Id);

            if (startDate.HasValue)
                query = query.Where(t => t.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(t => t.CreatedAt <= endDate.Value);

            return await query.ToListAsync();
        }
    }
}
