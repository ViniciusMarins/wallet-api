using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    [Index(nameof(Cpf), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Wallet Wallet { get; set; }
        [Required]
        public string Cpf { get; set; }
    }
}
