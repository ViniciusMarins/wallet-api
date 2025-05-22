using api.DTOs.Requests;
using api.DTOs.Responses;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/v1/wallets")]
    public class WalletController(IWalletService walletService, ITransactionService transactionService) : ControllerBase
    {
        private readonly IWalletService _walletService = walletService;
        private readonly ITransactionService _transactionService = transactionService;

        [HttpGet("{code}/balance")]
        public async Task<IActionResult> GetBalance([FromRoute] string code)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var wallet = await _walletService.FindByCode(code);
            if (wallet == null || wallet.UserId != userId)
                return NotFound(new { message = "Wallet not found or does not belong to the user." });

            var balance = await _walletService.GetBalance(code);
            return Ok(new { balance });
        }

        [HttpPost("{code}/deposit")]
        public async Task<IActionResult> Deposit([FromRoute] string code, [FromBody] DepositDTO depositDTO)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var wallet = await _walletService.FindByCode(code);
            if (wallet == null || wallet.UserId != userId)
                return NotFound(new { message = "Wallet not found or does not belong to the user." });

            wallet = await _walletService.Deposit(wallet, depositDTO.Amount);
            return Ok(new { message = "Deposit completed successfully.", balance = wallet.Balance });
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDTO transferDTO)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var fromWallet = await _walletService.FindByCode(transferDTO.FromWalletCode);
            if (fromWallet == null || fromWallet.UserId != userId)
                return NotFound(new { message = "Source wallet not found or does not belong to the user." });

            var toWallet = await _walletService.FindByCode(transferDTO.ToWalletCode);
            if (toWallet == null)
                return NotFound(new { message = "Destination wallet not found." });

            await _walletService.Transfer(transferDTO.Amount, fromWallet, toWallet);
            return Ok(new { message = "Transfer completed successfully." });
        }

        [HttpGet("{code}/transactions")]
        public async Task<IActionResult> GetTransactions([FromRoute] string code, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var wallet = await _walletService.FindByCode(code);
            if (wallet == null || wallet.UserId != userId)
                return NotFound(new { message = "Wallet not found or does not belong to the user." });

            var transactions = await _transactionService.GetTransactions(wallet, startDate, endDate);
            return Ok(transactions.Select(t => new GetTransactionDTO
            {
                CreatedAt = t.CreatedAt,
                Amount = t.Amount,
                TransactionType = t.TransactionType.ToString(),
                Status = t.Status.ToString(),
                FromWalletCode = t.FromWallet?.WalletCode!,
                ToWalletCode = t.ToWallet?.WalletCode!
            }));
        }
    }
}
