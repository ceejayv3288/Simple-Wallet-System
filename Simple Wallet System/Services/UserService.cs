using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Simple_Wallet_System.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Wallet_System.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;

        public UserService(ILogger<UserService> log, IConfiguration config)
        {
            _logger = log;
            _config = config;
        }

        public void ReadAll()
        {
            using (SqlConnection conn = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString")))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "Select * From Users";

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string loginName = reader["LoginName"].ToString();
                        string accountNumber = reader["AccountNumber"].ToString();

                        Console.WriteLine(loginName + " and " + accountNumber);
                    }
                    reader.Close();
                }
            }
            //Console.ReadKey();
        }

        public bool Register(Registration registration)
        {
            int resultReg = 0;
            bool isLoginNameExist = IsLoginNameExist(registration.LoginName);
            if (!isLoginNameExist)
            {
                resultReg = InsertRegistration(registration);
            }
            return resultReg == 1;
        }

        public bool IsLoginNameExist(string loginName)
        {
            bool result = false;
            string userExistQuery = $"Select * From Users where LoginName='{loginName}'";
            using (SqlConnection conn = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString")))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = userExistQuery;

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        result = true;
                    }

                    reader.Close();
                }
            }
            return result;
        }

        public int InsertRegistration(Registration registration)
        {
            registration.RegisterDate = DateTime.Now;

            int result = 0;
            string insertUserQuery = $"INSERT INTO Users (LoginName, AccountNumber, Password, Balance, RegisterDate)\r\nVALUES ('{registration.LoginName}', '{registration.AccountNumber}', HASHBYTES('SHA2_256','{registration.Password}'), {registration.Balance}, '{registration.RegisterDate}');\r\n";
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
            return result;
        }

        public User LoginUser(Login login)
        {
            User user = new User();
            string validateLogin = $"Select * From Users where LoginName='{login.LoginName}' and Password=HASHBYTES('SHA2_256','{login.Password}')";
            using (SqlConnection conn = new SqlConnection(_config.GetValue<string>("AppSettings:ConnectionString")))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = validateLogin;

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        user.Id = Convert.ToInt32(reader["Id"]);
                        user.LoginName = reader["LoginName"].ToString();
                        user.AccountNumber = reader["AccountNumber"].ToString();
                        user.Balance = Convert.ToInt32(reader["Balance"]);
                        user.RegisterDate = Convert.ToDateTime(reader["RegisterDate"]);
                    }

                    reader.Close();
                }
            }
            return user;
        }
    }
}
