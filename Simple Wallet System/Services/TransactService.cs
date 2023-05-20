using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Simple_Wallet_System.Constants;
using Simple_Wallet_System.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Wallet_System.Services
{
    public class TransactService : ITransactService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;

        public TransactService(ILogger<UserService> log, IConfiguration config)
        {
            _logger = log;
            _config = config;
        }

        public Tuple<bool, string> DepositWithdraw(string accountNumber, decimal amount, bool isDeposit)
        {
            decimal currentBalance = GetCurrentBalance(accountNumber);
            int result = 0;
            int recordResult = 0;
            string errorMessage = string.Empty;
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
                                if ((int)recordResult != 1)
                                {
                                    if (tran != null)
                                    {
                                        tran.Rollback();
                                    }
                                    errorMessage = "Error in saving transaction record.";
                                }
                            }
                            else
                            {
                                errorMessage = "Error in updating record.";
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

                return Tuple.Create(result == 1 && recordResult == 1, errorMessage);
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
            int recordResult = 0;
            string errorMessage = string.Empty;
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

                            if ((int)result == 1)
                            {
                                Transaction transaction = new Transaction
                                {
                                    TransactionType = (int)TransactionTypeEnum.Transfer,
                                    AccountNumbers = $"{senderAccountNumber}/{recipientAccountNumber}",
                                    Amount = amount,
                                    DateOfTransaction = DateTime.Now,
                                    EndBalance = $"Sender Balance: {updatedSenderBalance}/ Recipient Balance: {recipientCurrentBalance}"
                                };
                                recordResult = RecordTransaction(transaction);

                                if (recordResult != 1)
                                {
                                    errorMessage = "Error in saving transaction record.";
                                }
                            }
                            else
                            {
                                errorMessage = "Error in updating record.";
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

                return Tuple.Create(result == 1 && recordResult == 1, errorMessage);
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

        public int RecordTransaction(Transaction transaction)
        {
            int result = 0;
            string insertUserQuery = $"INSERT INTO Transactions(TransactionType, Amount, AccountNumbers, DateOfTransaction, EndBalance)\r\nVALUES ({transaction.TransactionType}, {transaction.Amount}, '{transaction.AccountNumbers}', '{transaction.DateOfTransaction}', '{transaction.EndBalance}');";
            using (SqlConnection conn = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString")))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = insertUserQuery;

                    result = cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            return (int)result;
        }

        public decimal GetCurrentBalance(string accountNumber)
        {
            decimal currentBalance = 0;
            string getBalanceQuery = $"Select * From Users where AccountNumber='{accountNumber}'";
            using (SqlConnection conn = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString")))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = getBalanceQuery;

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        currentBalance = Convert.ToInt32(reader["Balance"]);
                    }

                    reader.Close();
                }
            }
            return currentBalance;
        }
    }
}
