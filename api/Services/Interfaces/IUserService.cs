using api.DTOs.Requests;
using api.Models;

namespace api.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUser(CreateUserDTO userDTO);
        Task<User> FindByCpf(string cpf);
    }
}
