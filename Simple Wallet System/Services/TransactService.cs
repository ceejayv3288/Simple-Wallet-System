using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Simple_Wallet_System.Constants;
using Simple_Wallet_System.Models;
using System.Data.SqlClient;

namespace Simple_Wallet_System.Services
{
    public class TransactService : ITransactService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;
        private readonly IDbService _dbService;

        public TransactService(ILogger<UserService> log, IConfiguration config, IDbService dbService)
        {
            _logger = log;
            _config = config;
            _dbService = dbService;
        }

        public Tuple<bool, string> DepositWithdraw(string accountNumber, decimal amount, bool isDeposit)
        {
            decimal currentBalance = GetCurrentBalance(accountNumber);
            if (currentBalance < amount && !isDeposit)
            {
                return Tuple.Create(false, "Insufficient Balance");
            }
            int result = 0;
            bool recordResult = false;
            string message = string.Empty;
            decimal updatedBalance = isDeposit ? currentBalance + amount : currentBalance - amount;
            string updateBalanceQuery = $"\r\nUPDATE Users\r\nSET Balance = {updatedBalance}\r\nWHERE AccountNumber = '{accountNumber}'";
            SqlConnection conn = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString"));
            SqlTransaction tran = null;
            try
            {
                using (conn)
                {
                    conn.Open();
                    tran = conn.BeginTransaction("Deposit Transaction");
                    using (SqlCommand cmd = new SqlCommand(updateBalanceQuery, conn, tran))
                    {
                        try 
                        {
                            tran.Save("Deposit Query");
                            result = cmd.ExecuteNonQuery();

                            if ((int)result == 1)
                            {
                                Transaction transaction = new Transaction
                                {
                                    AccountNumbers = accountNumber,
                                    Amount = amount,
                                    DateOfTransaction = DateTime.Now,
                                    TransactionType = isDeposit ? (int)TransactionTypeEnum.Deposit : (int)TransactionTypeEnum.Withdraw,
                                    EndBalance = updatedBalance.ToString()
                                };
                                recordResult = RecordTransaction(transaction);
                                message = $"Success: New Balance: {updatedBalance}";
                                if (!recordResult)
                                {
                                    if (tran != null)
                                    {
                                        tran.Rollback();
                                    }
                                    message = "Error in saving transaction record.";
                                }
                            }
                            else
                            {
                                message = "Error in updating record.";
                            }
                            tran.Commit();
                        }
                        catch(Exception ex)
                        {
                            if (tran != null)
                            {
                                tran.Rollback();
                            }
                        }
                    }
                }

                return Tuple.Create(result == 1 && recordResult, message);
            }
            catch(Exception ex)
            {
                return Tuple.Create(false, ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        public Tuple<bool, string> TransferBalance(string senderAccountNumber, string recipientAccountNumber, decimal amount)
        {
            int result = 0;
            bool recordResult = false;
            string message = string.Empty;
            decimal senderCurrentBalance = GetCurrentBalance(senderAccountNumber);
            decimal recipientCurrentBalance = GetCurrentBalance(recipientAccountNumber);
            if (senderCurrentBalance < amount)
            {
                return Tuple.Create(false, "Insufficient Balance");
            }
            decimal updatedSenderBalance = senderCurrentBalance - amount;
            decimal updatedRecipientBalance = recipientCurrentBalance + amount;
            string updateBalancesQuery = $"\r\nUPDATE Users\r\nSET Balance = {updatedSenderBalance}\r\nWHERE AccountNumber = '{senderAccountNumber}'\r\nUPDATE Users\r\nSET Balance = {updatedRecipientBalance}\r\nWHERE AccountNumber = '{recipientAccountNumber}'";
            SqlConnection conn = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString"));
            SqlTransaction tran = null;
            try
            {
                using (conn)
                {
                    conn.Open();
                    tran = conn.BeginTransaction("Transfer Transaction");
                    using (SqlCommand cmd = new SqlCommand(updateBalancesQuery, conn, tran))
                    {
                        try
                        {
                            result = cmd.ExecuteNonQuery();

                            if ((int)result > 0)
                            {
                                Transaction transaction = new Transaction
                                {
                                    TransactionType = (int)TransactionTypeEnum.Transfer,
                                    AccountNumbers = $"{senderAccountNumber}/{recipientAccountNumber}",
                                    Amount = amount,
                                    DateOfTransaction = DateTime.Now,
                                    EndBalance = $"Sender Balance: {updatedSenderBalance}/ Recipient Balance: {updatedRecipientBalance}"
                                };
                                recordResult = RecordTransaction(transaction);
                                message = $"Success: Balance transfered from Account Number {senderAccountNumber} to {recipientAccountNumber}.\nUpdated Balance for sender is {updatedSenderBalance} while recipient is {updatedRecipientBalance}";
                                if (!recordResult)
                                {
                                    message = "Error in saving transaction record.";
                                }
                            }
                            else
                            {
                                message = "Error in updating record.";
                            }
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            if (tran != null)
                            {
                                tran.Rollback();
                            }
                        }
                    }
                }

                return Tuple.Create(result == 1 && recordResult, message);
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        public bool RecordTransaction(Transaction transaction)
        {
            bool result = false;
            string insertTransactionQuery = $"INSERT INTO Transactions(TransactionType, Amount, AccountNumbers, DateOfTransaction, EndBalance)\r\nOUTPUT INSERTED.* VALUES (@TransactionType, @Amount, @AccountNumbers, @DateOfTransaction, @EndBalance);";
            var transactionParam = new { TransactionType = transaction.TransactionType, Amount = transaction.Amount, AccountNumbers = transaction.AccountNumbers, DateOfTransaction = transaction.DateOfTransaction, EndBalance = transaction.EndBalance };
            //result = _dbService.Insert<Transaction>(insertTransactionQuery, transactionParam).Result != null;
            var ggg = _dbService.Insert<Transaction>(insertTransactionQuery, transactionParam).Result;
            result = ggg != null;
            //using (SqlConnection conn = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString")))
            //{
            //    conn.Open();
            //    using (SqlCommand cmd = new SqlCommand())
            //    {
            //        cmd.Connection = conn;
            //        cmd.CommandText = insertUserQuery;

            //        result = cmd.ExecuteNonQuery();

            //        conn.Close();
            //    }
            //}
            return result;
        }

        public decimal GetCurrentBalance(string accountNumber)
        {
            decimal currentBalance = 0;
            string getBalanceQuery = $"Select * From Users where AccountNumber=@AccountNumber";
            var accountNumberParam = new { AccountNumber = accountNumber };
            currentBalance = _dbService.GetAsync<User>(getBalanceQuery, accountNumberParam).Result.Balance;
            return currentBalance;
        }
    }
}
