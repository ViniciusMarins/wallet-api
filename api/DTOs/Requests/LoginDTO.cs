using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Requests
{
    public class LoginDTO
    {
        [Required]
        public string Cpf { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
