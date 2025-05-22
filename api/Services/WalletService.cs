using api.Data;
using api.Models;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using shortid;

namespace api.Services
{
    public class WalletService(AppDbContext context, ITransactionService transactionService) : IWalletService
    {
        public readonly AppDbContext _context = context;
        public readonly ITransactionService _transactionService = transactionService;

        public async Task<Wallet> FindByCode(string walletCode)
        {
            return await _context.Wallets.FirstOrDefaultAsync(x => x.WalletCode == walletCode);
        }

        public async Task<Wallet> Deposit(Wallet wallet, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("The amount must be greater than zero.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                wallet.Balance += amount;
                var newTransaction = _transactionService.CreateTransaction(TransactionType.DEPOSIT, amount, wallet, null);

                _context.Wallets.Update(wallet);
                _context.Transactions.Add(newTransaction);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return wallet;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task Transfer(decimal amount, Wallet fromWallet, Wallet toWallet)
        {
            if (amount <= 0)
                throw new ArgumentException("The wallet does not have enough balance to transfer {amount}.");
            if (fromWallet.Balance < amount)
                throw new InvalidOperationException($"The wallet does not have enough balance to transfer {amount}.");
            if (fromWallet.Id == toWallet.Id)
                throw new ArgumentException("The source and destination wallets cannot be the same.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                fromWallet.Balance -= amount;
                toWallet.Balance += amount;
                var newTransaction = _transactionService.CreateTransaction(TransactionType.TRANSFER, amount, fromWallet, toWallet);

                _context.Wallets.UpdateRange(fromWallet, toWallet);
                _context.Transactions.Add(newTransaction);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<decimal> GetBalance(string walletCode)
        {
            var wallet = await FindByCode(walletCode);
            if (wallet == null)
                throw new KeyNotFoundException("Wallet not found.");

            return wallet.Balance;
        }
    }
}
