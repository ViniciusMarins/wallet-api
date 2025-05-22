using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public string TimeZoneId { get; set; } 
        [Required]
        public int FromWalletId { get; set; } 
        public Wallet FromWallet { get; set; }
        public int? ToWalletId { get; set; }
        public Wallet ToWallet { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; } 
        [Required]
        public TransactionType TransactionType { get; set; }
        public TransactionStatus Status { get; set; } 
    }

    public enum TransactionType
    {
        DEPOSIT,    
        TRANSFER,
        WITHDRAW
    }

    public enum TransactionStatus
    {
        PENDING,
        COMPLETED,
        FAILED
    }
}