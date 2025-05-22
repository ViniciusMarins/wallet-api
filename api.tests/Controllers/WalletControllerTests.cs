using api.Controllers;
using api.DTOs.Requests;
using api.DTOs.Responses;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace api.Tests.Controllers
{
    public class WalletControllerTests
    {
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly Mock<ITransactionService> _transactionServiceMock;
        private readonly WalletController _controller;

        public WalletControllerTests()
        {
            _walletServiceMock = new Mock<IWalletService>();
            _transactionServiceMock = new Mock<ITransactionService>();
            _controller = new WalletController(_walletServiceMock.Object, _transactionServiceMock.Object);

            // Mock user authentication
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetBalance_WalletExists_ReturnsOkWithBalance()
        {
            // Arrange
            var walletCode = "WALLET_CODE";
            var wallet = new Wallet { Id = 1, WalletCode = walletCode, UserId = 1, Balance = 100.00m };

            _walletServiceMock.Setup(s => s.FindByCode(walletCode)).ReturnsAsync(wallet);
            _walletServiceMock.Setup(s => s.GetBalance(walletCode)).ReturnsAsync(100.00m);

            // Act
            var result = await _controller.GetBalance(walletCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = new AnonymousObject(okResult.Value!);
            Assert.Equal(100.00m, response.GetProperty<decimal>("balance"));
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetBalance_WalletNotFound_ReturnsNotFound()
        {
            // Arrange
            var walletCode = "INVALID_CODE";

            _walletServiceMock.Setup(s => s.FindByCode(walletCode))!.ReturnsAsync(null as Wallet);

            // Act
            var result = await _controller.GetBalance(walletCode);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = new AnonymousObject(notFoundResult.Value!);
            Assert.Equal("Wallet not found or does not belong to the user.", response.GetProperty<string>("message"));
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Deposit_WalletExists_ReturnsOkWithBalance()
        {
            // Arrange
            var walletCode = "WALLET_CODE";
            var depositDTO = new DepositDTO { Amount = 50.00m };
            var wallet = new Wallet { Id = 1, WalletCode = walletCode, UserId = 1, Balance = 100.00m };
            var updatedWallet = new Wallet { Id = 1, WalletCode = walletCode, UserId = 1, Balance = 150.00m };

            _walletServiceMock.Setup(s => s.FindByCode(walletCode)).ReturnsAsync(wallet);
            _walletServiceMock.Setup(s => s.Deposit(wallet, depositDTO.Amount)).ReturnsAsync(updatedWallet);

            // Act
            var result = await _controller.Deposit(walletCode, depositDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = new AnonymousObject(okResult.Value!);
            Assert.Equal("Deposit completed successfully.", response.GetProperty<string>("message"));
            Assert.Equal(150.00m, response.GetProperty<decimal>("balance"));
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task Deposit_WalletNotFound_ReturnsNotFound()
        {
            // Arrange
            var walletCode = "INVALID_CODE";
            var depositDTO = new DepositDTO { Amount = 50.00m };

            _walletServiceMock.Setup(s => s.FindByCode(walletCode))!.ReturnsAsync(null as Wallet);

            // Act
            var result = await _controller.Deposit(walletCode, depositDTO);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = new AnonymousObject(notFoundResult.Value!);
            Assert.Equal("Wallet not found or does not belong to the user.", response.GetProperty<string>("message"));
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Transfer_ValidWallets_ReturnsOk()
        {
            // Arrange
            var transferDTO = new TransferDTO
            {
                Amount = 50.00m,
                FromWalletCode = "FROM_WALLET",
                ToWalletCode = "TO_WALLET"
            };
            var fromWallet = new Wallet { Id = 1, WalletCode = "FROM_WALLET", UserId = 1, Balance = 100.00m };
            var toWallet = new Wallet { Id = 2, WalletCode = "TO_WALLET", UserId = 2, Balance = 0.00m };

            _walletServiceMock.Setup(s => s.FindByCode("FROM_WALLET")).ReturnsAsync(fromWallet);
            _walletServiceMock.Setup(s => s.FindByCode("TO_WALLET")).ReturnsAsync(toWallet);
            _walletServiceMock.Setup(s => s.Transfer(transferDTO.Amount, fromWallet, toWallet)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Transfer(transferDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = new AnonymousObject(okResult.Value!);
            Assert.Equal("Transfer completed successfully.", response.GetProperty<string>("message"));
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task Transfer_SourceWalletNotFound_ReturnsNotFound()
        {
            // Arrange
            var transferDTO = new TransferDTO
            {
                Amount = 50.00m,
                FromWalletCode = "INVALID_WALLET",
                ToWalletCode = "TO_WALLET"
            };

            _walletServiceMock.Setup(s => s.FindByCode("INVALID_WALLET"))!.ReturnsAsync(null as Wallet);

            // Act
            var result = await _controller.Transfer(transferDTO);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = new AnonymousObject(notFoundResult.Value);
            Assert.Equal("Source wallet not found or does not belong to the user.", response.GetProperty<string>("message"));
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task Transfer_DestinationWalletNotFound_ReturnsNotFound()
        {
            // Arrange
            var transferDTO = new TransferDTO
            {
                Amount = 50.00m,
                FromWalletCode = "FROM_WALLET",
                ToWalletCode = "INVALID_WALLET"
            };
            var fromWallet = new Wallet { Id = 1, WalletCode = "FROM_WALLET", UserId = 1, Balance = 100.00m };

            _walletServiceMock.Setup(s => s.FindByCode("FROM_WALLET")).ReturnsAsync(fromWallet);
            _walletServiceMock.Setup(s => s.FindByCode("INVALID_WALLET"))!.ReturnsAsync(null as Wallet);

            // Act
            var result = await _controller.Transfer(transferDTO);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = new AnonymousObject(notFoundResult.Value);
            Assert.Equal("Destination wallet not found.", response.GetProperty<string>("message"));
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetTransactions_WalletExists_ReturnsOkWithTransactions()
        {
            // Arrange
            var walletCode = "WALLET_CODE";
            var wallet = new Wallet { Id = 1, WalletCode = walletCode, UserId = 1 };

            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = 1,
                    CreatedAt = new DateTime(2025, 5, 11, 4, 0, 0, DateTimeKind.Utc),
                    Amount = 100.00m,
                    TransactionType = TransactionType.DEPOSIT,
                    Status = TransactionStatus.COMPLETED,
                    FromWallet = wallet,
                    ToWallet = null!
                }
            };

            _walletServiceMock.Setup(s => s.FindByCode(walletCode)).ReturnsAsync(wallet);
            _transactionServiceMock.Setup(s => s.GetTransactions(wallet, null, null)).ReturnsAsync(transactions);

            // Act
            var result = await _controller.GetTransactions(walletCode, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var transactionDtos = Assert.IsAssignableFrom<IEnumerable<GetTransactionDTO>>(okResult.Value);
            var dto = transactionDtos.First();

            Assert.Equal(new DateTime(2025, 5, 11, 4, 0, 0, DateTimeKind.Utc), dto.CreatedAt);
            Assert.Equal(100.00m, dto.Amount);
            Assert.Equal("DEPOSIT", dto.TransactionType);
            Assert.Equal("COMPLETED", dto.Status);
            Assert.Equal(walletCode, dto.FromWalletCode);
            Assert.Null(dto.ToWalletCode);

            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
        }

        [Fact]
        public async Task GetTransactions_WalletNotFound_ReturnsNotFound()
        {
            // Arrange
            var walletCode = "INVALID_CODE";
            _walletServiceMock.Setup(s => s.FindByCode(walletCode))!.ReturnsAsync(null as Wallet);

            // Act
            var result = await _controller.GetTransactions(walletCode, null, null);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = new AnonymousObject(notFoundResult.Value!);
            Assert.Equal("Wallet not found or does not belong to the user.", response.GetProperty<string>("message"));
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        private class AnonymousObject
        {
            private readonly object _obj;

            public AnonymousObject(object obj)
            {
                _obj = obj;
            }

            public T GetProperty<T>(string propertyName)
            {
                var property = _obj.GetType().GetProperty(propertyName);
                return (T)property!.GetValue(_obj)!;
            }
        }
    }
}