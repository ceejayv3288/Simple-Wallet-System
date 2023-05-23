using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Simple_Wallet_System.Models;

namespace Simple_Wallet_System.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;
        private readonly IDbService _dbService;

        public UserService(ILogger<UserService> log, IConfiguration config, IDbService dbService)
        {
            _logger = log;
            _config = config;
            _dbService = dbService;
        }

        public bool Register(Registration registration)
        {
            bool resultReg = false;
            bool isLoginNameExist = IsLoginNameExist(registration.LoginName);
            if (!isLoginNameExist)
            {
                resultReg = InsertRegistration(registration);
            }
            return resultReg;
        }

        public bool IsLoginNameExist(string loginName)
        {
            bool result = false;
            string userExistQuery = $"Select * From Users where LoginName=@LoginName";
            var loginParam = new { LoginName = loginName };
            result = _dbService.GetAsync<User>(userExistQuery, loginParam).Result != null;
            return result;
        }

        public bool IsAccountNumberExist(string accountNumber)
        {
            bool result = false;
            string accountNumberExistQuery = $"Select * From Users where AccountNumber=@AccountNumber";
            var accountParam = new { AccountNumber = accountNumber };
            result = _dbService.GetAsync<User>(accountNumberExistQuery, accountParam).Result != null;
            return result;
        }

        public bool InsertRegistration(Registration registration)
        {
            Random rnd = new Random();
            bool result = false;
            registration.RegisterDate = DateTime.Now;
            string generatedAccountNumber = string.Empty;
            bool isAccountNumberExist = true;
            while (isAccountNumberExist)
            {
                int random = rnd.Next(1, 9999);
                generatedAccountNumber = $"{registration.RegisterDate.Year}{registration.RegisterDate.Month.ToString("00")}{registration.RegisterDate.Day.ToString("00")}{random}";
                isAccountNumberExist = IsAccountNumberExist(generatedAccountNumber);
            }
            registration.AccountNumber = generatedAccountNumber;
            string insertUserQuery = $"INSERT INTO Users (LoginName, AccountNumber, Password, Balance, RegisterDate)\r\nOUTPUT INSERTED.* VALUES (@LoginName, @AccountNumber, HASHBYTES('SHA2_256',@Password), @Balance, @RegisterDate);\r\n";
            var accountParam = new { LoginName = registration.LoginName, AccountNumber = registration.AccountNumber, Password = registration.Password, Balance = registration.Balance, RegisterDate = registration.RegisterDate};
            result = _dbService.Insert<User>(insertUserQuery, accountParam).Result != null;
            return result;
        }

        public User LoginUser(Login login)
        {
            User user = new User();
            string validateLoginQuery = $"Select * From Users where LoginName=@LoginName and Password=HASHBYTES('SHA2_256',@Password)";
            var accountParam = new { LoginName = login.LoginName, Password = login.Password };
            user = _dbService.GetAsync<User>(validateLoginQuery, accountParam).Result;
            return user;
        }
    }
}
