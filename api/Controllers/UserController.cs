using api.Data;
using api.DTOs.Requests;
using api.Models;
using api.Services;
using api.Services.Interfaces;
using api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using shortid;

namespace api.Controllers
{
    [ApiController]
    [Route("/api/v1/users")]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO userDTO)
        {
            var user = await _userService.CreateUser(userDTO);

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "User created successfully.",
                name = user.Name,
                cpf = user.Cpf,
                walletCode = user.Wallet.WalletCode
            });
        }
    }
}
