using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Requests
{
    public class DepositDTO
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }

    public class WithdrawDTO
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }

    public class TransferDTO
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string FromWalletCode { get; set; }

        [Required]
        public string ToWalletCode { get; set; }
    }
}
