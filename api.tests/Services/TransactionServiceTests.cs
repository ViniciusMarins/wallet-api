using api.Data;
using api.Models;
using api.Services;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace api.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly AppDbContext _context;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            // Configura o banco de dados em memória
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _transactionService = new TransactionService(_context);
        }

        [Fact]
        public void CreateTransaction_Deposit_ReturnsTransactionWithCorrectProperties()
        {
            // Arrange
            var fromWallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var amount = 50.00m;
            var transactionType = TransactionType.DEPOSIT;

            // Act
            var transaction = _transactionService.CreateTransaction(transactionType, amount, fromWallet, null);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(amount, transaction.Amount);
            Assert.Equal(fromWallet.Id, transaction.FromWalletId);
            Assert.Null(transaction.ToWalletId);
            Assert.Equal(transactionType, transaction.TransactionType);
            Assert.Equal(TransactionStatus.COMPLETED, transaction.Status);
            Assert.True(DateTime.UtcNow.Subtract(transaction.CreatedAt).TotalSeconds < 1); // Verifica se CreatedAt é recente
        }

        [Fact]
        public void CreateTransaction_Transfer_ReturnsTransactionWithCorrectProperties()
        {
            // Arrange
            var fromWallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var toWallet = new Wallet { Id = 2, WalletCode = "WALLET2", UserId = 2, Balance = 0.00m };
            var amount = 50.00m;
            var transactionType = TransactionType.TRANSFER;

            // Act
            var transaction = _transactionService.CreateTransaction(transactionType, amount, fromWallet, toWallet);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(amount, transaction.Amount);
            Assert.Equal(fromWallet.Id, transaction.FromWalletId);
            Assert.Equal(toWallet.Id, transaction.ToWalletId);
            Assert.Equal(transactionType, transaction.TransactionType);
            Assert.Equal(TransactionStatus.COMPLETED, transaction.Status);
            Assert.True(DateTime.UtcNow.Subtract(transaction.CreatedAt).TotalSeconds < 1);
        }

        [Fact]
        public async Task GetTransactions_NoFilters_ReturnsAllTransactionsForWallet()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var otherWallet = new Wallet { Id = 2, WalletCode = "WALLET2", UserId = 2, Balance = 0.00m };
            var transactions = new List<Transaction>
            {
                new ()
                {
                    Id = 1,
                    Amount = 100.00m,
                    FromWalletId = wallet.Id,
                    TimeZoneId = "Brasilia/SP",
                    ToWalletId = null,
                    CreatedAt = new DateTime(2025, 5, 11, 4, 0, 0, DateTimeKind.Utc),
                    TransactionType = TransactionType.DEPOSIT,
                    Status = TransactionStatus.COMPLETED,
                    FromWallet = wallet
                },
                new ()
                {
                    Id = 2,
                    Amount = 50.00m,
                    FromWalletId = wallet.Id,
                    TimeZoneId = "Brasilia/SP",
                    ToWalletId = otherWallet.Id,
                    CreatedAt = new DateTime(2025, 5, 12, 4, 0, 0, DateTimeKind.Utc),
                    TransactionType = TransactionType.TRANSFER,
                    Status = TransactionStatus.COMPLETED,
                    FromWallet = wallet,
                    ToWallet = otherWallet
                }
            };

            await _context.Wallets.AddAsync(wallet);
            await _context.Wallets.AddAsync(otherWallet);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            // Act
            var result = await _transactionService.GetTransactions(wallet, null, null);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Id == 1 && t.TransactionType == TransactionType.DEPOSIT);
            Assert.Contains(result, t => t.Id == 2 && t.TransactionType == TransactionType.TRANSFER);
            Assert.All(result, t => Assert.NotNull(t.FromWallet));
            Assert.Contains(result, t => t.ToWallet != null && t.ToWallet.Id == otherWallet.Id);
        }

        [Fact]
        public async Task GetTransactions_WithStartDateFilter_ReturnsFilteredTransactions()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var transactions = new List<Transaction>
            {
                new ()
                {
                    Id = 1,
                    Amount = 100.00m,
                    FromWalletId = wallet.Id,
                    TimeZoneId = "Brasilia/SP",
                    ToWalletId = null,
                    CreatedAt = new DateTime(2025, 5, 10, 4, 0, 0, DateTimeKind.Utc),
                    TransactionType = TransactionType.DEPOSIT,
                    Status = TransactionStatus.COMPLETED,
                    FromWallet = wallet
                },
                new ()
                {
                    Id = 2,
                    Amount = 50.00m,
                    FromWalletId = wallet.Id,
                    TimeZoneId = "Brasilia/SP",
                    ToWalletId = null,
                    CreatedAt = new DateTime(2025, 5, 12, 4, 0, 0, DateTimeKind.Utc),
                    TransactionType = TransactionType.DEPOSIT,
                    Status = TransactionStatus.COMPLETED,
                    FromWallet = wallet
                }
            };

            await _context.Wallets.AddAsync(wallet);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            var startDate = new DateTime(2025, 5, 11, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = await _transactionService.GetTransactions(wallet, startDate, null);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result.First().Id);
            Assert.Equal(new DateTime(2025, 5, 12, 4, 0, 0, DateTimeKind.Utc), result.First().CreatedAt);
        }

        [Fact]
        public async Task GetTransactions_WithEndDateFilter_ReturnsFilteredTransactions()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var transactions = new List<Transaction>
            {
                new ()
                {
                    Id = 1,
                    Amount = 100.00m,
                    FromWalletId = wallet.Id,
                    TimeZoneId = "Brasilia/SP",
                    ToWalletId = null,
                    CreatedAt = new DateTime(2025, 5, 10, 4, 0, 0, DateTimeKind.Utc),
                    TransactionType = TransactionType.DEPOSIT,
                    Status = TransactionStatus.COMPLETED,
                    FromWallet = wallet
                },
                new ()
                {
                    Id = 2,
                    Amount = 50.00m,
                    FromWalletId = wallet.Id,
                    TimeZoneId = "Brasilia/SP",
                    ToWalletId = null,
                    CreatedAt = new DateTime(2025, 5, 12, 4, 0, 0, DateTimeKind.Utc),
                    TransactionType = TransactionType.DEPOSIT,
                    Status = TransactionStatus.COMPLETED,
                    FromWallet = wallet
                }
            };

            await _context.Wallets.AddAsync(wallet);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            var endDate = new DateTime(2025, 5, 11, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = await _transactionService.GetTransactions(wallet, null, endDate);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
            Assert.Equal(new DateTime(2025, 5, 10, 4, 0, 0, DateTimeKind.Utc), result.First().CreatedAt);
        }

        [Fact]
        public async Task GetTransactions_WithStartAndEndDateFilter_ReturnsFilteredTransactions()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var transactions = new List<Transaction>
            {
                new ()
                {
                    Id = 1,
                    Amount = 100.00m,
                    FromWalletId = wallet.Id,
                    TimeZoneId = "Brasilia/SP",
                    ToWalletId = null,
                    CreatedAt = new DateTime(2025, 5, 10, 4, 0, 0, DateTimeKind.Utc),
                    TransactionType = TransactionType.DEPOSIT,
                    Status = TransactionStatus.COMPLETED,
                    FromWallet = wallet
                },
                new ()
                {
                    Id = 2,
                    Amount = 50.00m,
                    FromWalletId = wallet.Id,
                    TimeZoneId = "Brasilia/SP",
                    ToWalletId = null,
                    CreatedAt = new DateTime(2025, 5, 11, 4, 0, 0, DateTimeKind.Utc),
                    TransactionType = TransactionType.DEPOSIT,
                    Status = TransactionStatus.COMPLETED,
                    FromWallet = wallet
                },
                new ()
                {
                    Id = 3,
                    Amount = 25.00m,
                    FromWalletId = wallet.Id,
                    TimeZoneId = "Brasilia/SP",
                    ToWalletId = null,
                    CreatedAt = new DateTime(2025, 5, 12, 4, 0, 0, DateTimeKind.Utc),
                    TransactionType = TransactionType.DEPOSIT,
                    Status = TransactionStatus.COMPLETED,
                    FromWallet = wallet
                }
            };

            await _context.Wallets.AddAsync(wallet);
            await _context.Transactions.AddRangeAsync(transactions);
            await _context.SaveChangesAsync();

            var startDate = new DateTime(2025, 5, 11, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2025, 5, 11, 23, 59, 59, DateTimeKind.Utc);

            // Act
            var result = await _transactionService.GetTransactions(wallet, startDate, endDate);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result.First().Id);
            Assert.Equal(new DateTime(2025, 5, 11, 4, 0, 0, DateTimeKind.Utc), result.First().CreatedAt);
        }

        [Fact]
        public async Task GetTransactions_NoTransactions_ReturnsEmptyList()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();

            // Act
            var result = await _transactionService.GetTransactions(wallet, null, null);

            // Assert
            Assert.Empty(result);
        }
    }
}