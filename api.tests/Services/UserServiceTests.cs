using api.Data;
using api.DTOs.Requests;
using api.Models;
using api.Services;
using api.Utils;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace api.Tests.Services
{
    public class UserServiceTests
    {
        private readonly AppDbContext _context;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _userService = new UserService(_context);
        }

        [Fact]
        public async Task FindByCpf_UserExists_ReturnsUser()
        {
            // Arrange
            var cpf = "1111111111";
            var user = new User
            {
                Id = 1,
                Name = "Vinicius",
                Cpf = cpf,
                Email = "v@gmail.com",
                Password = "hashedpassword",
                Wallet = new Wallet { Id = 1, WalletCode = "WALLET1" }
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.FindByCpf(cpf);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cpf, result.Cpf);
            Assert.Equal("Vinicius", result.Name);
            Assert.NotNull(result.Wallet);
        }

        [Fact]
        public async Task FindByCpf_UserNotFound_ReturnsNull()
        {
            // Arrange
            var cpf = "9999999999";

            // Act
            var result = await _userService.FindByCpf(cpf);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUser_ValidDTO_CreatesUserWithWallet()
        {
            // Arrange
            var userDTO = new CreateUserDTO
            {
                Name = "Vinicius",
                Cpf = "1111111111",
                Email = "v@gmail.com",
                Password = "test123"
            };

            // Act
            var result = await _userService.CreateUser(userDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userDTO.Name, result.Name);
            Assert.Equal(userDTO.Cpf, result.Cpf);
            Assert.Equal(userDTO.Email, result.Email);
            Assert.NotEqual(userDTO.Password, result.Password); // Verifica que a senha foi hasheada
            Assert.NotNull(result.Wallet);
            Assert.False(string.IsNullOrEmpty(result.Wallet.WalletCode));
            Assert.Equal(0, result.Wallet.Balance);

            // Verifica no banco
            var dbUser = await _context.Users.Include(u => u.Wallet).FirstOrDefaultAsync(u => u.Cpf == userDTO.Cpf);
            Assert.NotNull(dbUser);
            Assert.Equal(result.Wallet.WalletCode, dbUser.Wallet.WalletCode);
        }
    }
}