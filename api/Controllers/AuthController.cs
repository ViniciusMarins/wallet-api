using api.Data;
using api.DTOs.Requests;
using api.Services;
using api.Services.Interfaces;
using api.Utils;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("/api/v1/auth")]
    public class AuthController(ITokenService tokenService, IUserService userService) : ControllerBase
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly IUserService _userService = userService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var user = await _userService.FindByCpf(loginDTO.Cpf);
            if (user == null || !HashUtils.VerifyPassword(loginDTO.Password, user.Password))
            {
                return Unauthorized(new { message = "Cpf or password invalid." });
            }

            var token = _tokenService.CreateToken(user);
            return Ok(new { token });
        }
    }
}
