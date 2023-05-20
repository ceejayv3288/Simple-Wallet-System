namespace Simple_Wallet_System.Services
{
    public interface ITransactService
    {
        public Tuple<bool, string> DepositWithdraw(string accountNumber, decimal amount, bool isDeposit);
        public Tuple<bool, string> TransferBalance(string senderAccountNumber, string recipientAccountNumber, decimal amount);
    }
}