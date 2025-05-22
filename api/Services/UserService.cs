using api.Data;
using api.DTOs.Requests;
using api.Models;
using api.Services.Interfaces;
using api.Utils;
using Microsoft.EntityFrameworkCore;
using shortid;

namespace api.Services
{
    public class UserService(AppDbContext context) : IUserService
    {
        private readonly AppDbContext _context = context;

        public async Task<User> FindByCpf(string cpf)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Cpf == cpf);
        }

        public async Task<User> CreateUser(CreateUserDTO userDTO)
        {
            var newUser = new User
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
                Cpf = userDTO.Cpf,
                Password = HashUtils.HashPassword(userDTO.Password),
                Wallet = new Wallet
                {
                    WalletCode = ShortId.Generate()
                }
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }
    }
}
