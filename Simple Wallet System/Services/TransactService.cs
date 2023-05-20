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

        public int DepositWithdraw(string accountNumber, decimal amount, bool isDeposit)
        {
            decimal currentBalance = GetCurrentBalance(accountNumber);
            int result = 0;
            decimal updatedBalance = isDeposit ? currentBalance + amount : currentBalance - amount;
            string updateBalanceQuery = $"\r\nUPDATE Users\r\nSET Balance = {updatedBalance}\r\nWHERE AccountNumber = {accountNumber}";
            using (SqlConnection conn = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString")))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = updateBalanceQuery;

                    result = cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
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
                RecordTransaction(transaction);
            }

            return (int)result;
        }

        public int TransferBalance(string senderAccountNumber, string recipientAccountNumber, decimal amount)
        {
            int result = 0;
            decimal senderCurrentBalance = GetCurrentBalance(senderAccountNumber);
            decimal recipientCurrentBalance = GetCurrentBalance(recipientAccountNumber);
            if (senderCurrentBalance < amount)
            {

            }
            decimal updatedSenderBalance = senderCurrentBalance - amount;
            decimal updatedRecipientBalance = recipientCurrentBalance + amount;
            string updateBalancesQuery = $"\r\nUPDATE Users\r\nSET Balance = {updatedSenderBalance}\r\nWHERE AccountNumber = {senderAccountNumber}\r\nUPDATE Users\r\nSET Balance = {updatedRecipientBalance}\r\nWHERE AccountNumber = {recipientAccountNumber}";
            using (SqlConnection conn = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString")))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = updateBalancesQuery;

                    result = cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
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
                int recordResult = RecordTransaction(transaction);
            }
            return (int)result;
        }

        public int RecordTransaction(Transaction transaction)
        {
            int result = 0;
            string insertUserQuery = $"INSERT INTO Transactions(TransactionType, Amount, AccountNumbers, DateOfTransaction, EndBalance)\r\nVALUES ({transaction.TransactionType}, {transaction.Amount}, '{transaction.AccountNumbers}', {transaction.DateOfTransaction}, '{transaction.EndBalance}');";
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
