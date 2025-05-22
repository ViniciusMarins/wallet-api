using api.Models;

namespace api.Services.Interfaces
{
    public interface IWalletService
    {
        Task<Wallet> Deposit(Wallet wallet, decimal amount);
        Task<Wallet> FindByCode(string walletCode);
        Task<decimal> GetBalance(string walletCode);
        Task Transfer(decimal amount, Wallet fromWallet, Wallet toWallet);
    }
}
