using api.Models;
using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Responses
{
    public class GetTransactionDTO
    {
        public DateTime CreatedAt { get; set; }
        public string FromWalletCode { get; set; }
        public string ToWalletCode { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
    }
}
