
using api.Data;
using api.Models;
using api.Services;
using api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using Xunit;

namespace api.Tests.Services
{
    public class WalletServiceTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly WalletService _walletService;

        public WalletServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // Suppress transaction warning
                .Options;
            _context = new AppDbContext(options);
            _transactionServiceMock = new Mock<ITransactionService>();
            _walletService = new WalletService(_context, _transactionServiceMock.Object);
        }

        [Fact]
        public async Task FindByCode_WalletExists_ReturnsWallet()
        {
            // Arrange
            var walletCode = "WALLET1";
            var wallet = new Wallet { Id = 1, WalletCode = walletCode, UserId = 1, Balance = 100.00m };
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();

            // Act
            var result = await _walletService.FindByCode(walletCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(walletCode, result.WalletCode);
            Assert.Equal(100.00m, result.Balance);
        }

        [Fact]
        public async Task FindByCode_WalletNotFound_ReturnsNull()
        {
            // Arrange
            var walletCode = "INVALID";

            // Act
            var result = await _walletService.FindByCode(walletCode);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Deposit_ValidAmount_UpdatesBalanceAndCreatesTransaction()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var amount = 50.00m;
            var transaction = new Transaction
            {
                Id = 1,
                Amount = amount,
                FromWalletId = wallet.Id,
                TimeZoneId = "Brasilia/SP",
                TransactionType = TransactionType.DEPOSIT,
                Status = TransactionStatus.COMPLETED
            };
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();
            _transactionServiceMock.Setup(s => s.CreateTransaction(TransactionType.DEPOSIT, amount, wallet, null))
                .Returns(transaction);

            // Act
            var result = await _walletService.Deposit(wallet, amount);

            // Assert
            Assert.Equal(150.00m, result.Balance);
            var dbWallet = await _context.Wallets.FindAsync(wallet.Id);
            Assert.Equal(150.00m, dbWallet.Balance);
            _transactionServiceMock.Verify(s => s.CreateTransaction(TransactionType.DEPOSIT, amount, wallet, null), Times.Once());
            Assert.Single(_context.Transactions);
        }

        [Fact]
        public async Task Deposit_NegativeAmount_ThrowsArgumentException()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var amount = -10.00m;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _walletService.Deposit(wallet, amount));
            Assert.Equal(100.00m, wallet.Balance); // Saldo não deve mudar
        }

        [Fact]
        public async Task Transfer_ValidAmount_UpdatesBalancesAndCreatesTransaction()
        {
            // Arrange
            var fromWallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var toWallet = new Wallet { Id = 2, WalletCode = "WALLET2", UserId = 2, Balance = 50.00m };
            var amount = 30.00m;
            var transaction = new Transaction
            {
                Id = 1,
                Amount = amount,
                FromWalletId = fromWallet.Id,
                TimeZoneId = "Brasilia/SP",
                ToWalletId = toWallet.Id,
                TransactionType = TransactionType.TRANSFER,
                Status = TransactionStatus.COMPLETED
            };
            await _context.Wallets.AddRangeAsync(fromWallet, toWallet);
            await _context.SaveChangesAsync();
            _transactionServiceMock.Setup(s => s.CreateTransaction(TransactionType.TRANSFER, amount, fromWallet, toWallet))
                .Returns(transaction);

            // Act
            await _walletService.Transfer(amount, fromWallet, toWallet);

            // Assert
            var dbFromWallet = await _context.Wallets.FindAsync(fromWallet.Id);
            var dbToWallet = await _context.Wallets.FindAsync(toWallet.Id);
            Assert.Equal(70.00m, dbFromWallet.Balance);
            Assert.Equal(80.00m, dbToWallet.Balance);
            _transactionServiceMock.Verify(s => s.CreateTransaction(TransactionType.TRANSFER, amount, fromWallet, toWallet), Times.Once());
            Assert.Single(_context.Transactions);
        }

        [Fact]
        public async Task Transfer_NegativeAmount_ThrowsArgumentException()
        {
            // Arrange
            var fromWallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var toWallet = new Wallet { Id = 2, WalletCode = "WALLET2", UserId = 2, Balance = 50.00m };
            var amount = -10.00m;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _walletService.Transfer(amount, fromWallet, toWallet));
            Assert.Equal(100.00m, fromWallet.Balance);
            Assert.Equal(50.00m, toWallet.Balance);
        }

        [Fact]
        public async Task Transfer_InsufficientBalance_ThrowsInvalidOperationException()
        {
            // Arrange
            var fromWallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 20.00m };
            var toWallet = new Wallet { Id = 2, WalletCode = "WALLET2", UserId = 2, Balance = 50.00m };
            var amount = 30.00m;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _walletService.Transfer(amount, fromWallet, toWallet));
            Assert.Equal(20.00m, fromWallet.Balance);
            Assert.Equal(50.00m, toWallet.Balance);
        }

        [Fact]
        public async Task Transfer_SameWallet_ThrowsArgumentException()
        {
            // Arrange
            var wallet = new Wallet { Id = 1, WalletCode = "WALLET1", UserId = 1, Balance = 100.00m };
            var amount = 30.00m;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _walletService.Transfer(amount, wallet, wallet));
            Assert.Equal(100.00m, wallet.Balance);
        }

        [Fact]
        public async Task GetBalance_WalletExists_ReturnsBalance()
        {
            // Arrange
            var walletCode = "WALLET1";
            var wallet = new Wallet { Id = 1, WalletCode = walletCode, UserId = 1, Balance = 100.00m };
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();

            // Act
            var balance = await _walletService.GetBalance(walletCode);

            // Assert
            Assert.Equal(100.00m, balance);
        }

        [Fact]
        public async Task GetBalance_WalletNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var walletCode = "INVALID";

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _walletService.GetBalance(walletCode));
        }
    }
}