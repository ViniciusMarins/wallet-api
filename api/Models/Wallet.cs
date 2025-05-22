using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    [Index(nameof(WalletCode), IsUnique = true)]
    public class Wallet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string WalletCode { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Balance { get; set; } = 0M; // Saldo da carteira
    }
}