using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Requests
{
    public class CreateUserDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } // Armazenará o hash da senha

        [Required]
        [MaxLength(11)]
        public string Cpf { get; set; }
    }
}
