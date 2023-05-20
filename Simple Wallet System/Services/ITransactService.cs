namespace Simple_Wallet_System.Services
{
    public interface ITransactService
    {
        public int DepositWithdraw(string accountNumber, decimal amount, bool isDeposit);
    }
}