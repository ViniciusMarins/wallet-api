using api.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using api.Models;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using api.DTOs.Requests;
using api.Controllers;
using api.Services;
using System.Net;

namespace api.tests.Controllers
{
    public class UserControllerTests
    {
        [Fact]
        public async Task CreateUser()
        {
            // Arrange
            var user = new CreateUserDTO()
            {
                Name = "Vinicius",
                Cpf = "1111111111",
                Email = "v@gmail.com",
                Password = "test123"
            };

            var userCreated = new User()
            {
                Id = 1,
                Name = "Vinicius",
                Cpf = "1111111111",
                Email = "v@gmail.com",
                Password = "test123",
                Wallet = new()
                {
                    Id = 1,
                    Balance = 0,
                    WalletCode = "ASDW2132_XA"
                }
            };

            var userServiceMock = new Mock<IUserService>();
            userServiceMock.Setup(x => x.CreateUser(It.IsAny<CreateUserDTO>())).ReturnsAsync(userCreated);

            var userController = new UserController(userServiceMock.Object);

            // Act
            var result = await userController.CreateUser(user);

            // Assert
            Assert.NotNull(result);
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }
    }
}
