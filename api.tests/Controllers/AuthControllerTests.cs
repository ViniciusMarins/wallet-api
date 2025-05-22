using api.Controllers;
using api.DTOs.Requests;
using api.Models;
using api.Services.Interfaces;
using api.Utils;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _tokenServiceMock = new Mock<ITokenService>();
            _userServiceMock = new Mock<IUserService>();
            _controller = new AuthController(_tokenServiceMock.Object, _userServiceMock.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Cpf = "1111111111",
                Password = "test123"
            };
            var user = new User
            {
                Id = 1,
                Cpf = loginDTO.Cpf,
                Password = HashUtils.HashPassword(loginDTO.Password) // Simula senha hasheada
            };
            var token = "jwt-token";

            _userServiceMock.Setup(s => s.FindByCpf(loginDTO.Cpf)).ReturnsAsync(user);
            _tokenServiceMock.Setup(s => s.CreateToken(user)).Returns(token);

            // Act
            var result = await _controller.Login(loginDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.Equal(200, okResult.StatusCode);
            _userServiceMock.Verify(s => s.FindByCpf(loginDTO.Cpf), Times.Once());
            _tokenServiceMock.Verify(s => s.CreateToken(user), Times.Once());
        }

        [Fact]
        public async Task Login_InvalidCpf_ReturnsUnauthorized()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Cpf = "9999999999",
                Password = "test123"
            };

            _userServiceMock.Setup(s => s.FindByCpf(loginDTO.Cpf)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(loginDTO);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = unauthorizedResult.Value as dynamic;

            Assert.Equal(401, unauthorizedResult.StatusCode);
            _userServiceMock.Verify(s => s.FindByCpf(loginDTO.Cpf), Times.Once());
            _tokenServiceMock.Verify(s => s.CreateToken(It.IsAny<User>()), Times.Never());
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Cpf = "1111111111",
                Password = "wrongpassword"
            };
            var user = new User
            {
                Id = 1,
                Cpf = loginDTO.Cpf,
                Password = HashUtils.HashPassword("test123") // Senha correta hasheada
            };

            _userServiceMock.Setup(s => s.FindByCpf(loginDTO.Cpf)).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(loginDTO);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = unauthorizedResult.Value as dynamic;

            Assert.Equal(401, unauthorizedResult.StatusCode);
            _userServiceMock.Verify(s => s.FindByCpf(loginDTO.Cpf), Times.Once());
            _tokenServiceMock.Verify(s => s.CreateToken(It.IsAny<User>()), Times.Never());
        }
    }
}