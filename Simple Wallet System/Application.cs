using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Simple_Wallet_System.Constants;
using Simple_Wallet_System.Models;
using Simple_Wallet_System.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Wallet_System
{
    public class Application
    {
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly ITransactService _transactService;
        private User _user;

        public Application(ILogger<UserService> log, IConfiguration config, IUserService userService, ITransactService transactService)
        {
            _logger = log;
            _config = config;
            _userService = userService;
            _transactService = transactService;
        }

        public void PrintOptions()
        {
            bool validInput = false;
            Console.WriteLine("Simple Wallet System\nChoose from the options below:\n");

            while (!validInput)
            {
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register");
                ConsoleKeyInfo level = Console.ReadKey(true);
                switch (level.KeyChar)
                {
                    case (char)ConsoleKey.D1:
                        Login login = new Login();
                        while (String.IsNullOrWhiteSpace(login.LoginName))
                        {
                            Console.WriteLine("Enter Username: ");
                            login.LoginName = Console.ReadLine();
                        }
                        while (String.IsNullOrWhiteSpace(login.Password))
                        {
                            Console.WriteLine("Enter Password: ");
                            login.Password = Console.ReadLine();
                        }
                        _user = _userService.LoginUser(login);
                        if (_user.AccountNumber == null)
                        {
                            Console.WriteLine("Wrong Username or Password\n\n");
                        }
                        else
                        {
                            validInput = true;
                            UserOptions();
                        }
                        break;
                    case (char)ConsoleKey.D2:
                        bool passwordMatch = false;
                        Registration registration = new Registration();
                        Console.WriteLine("Enter Username: ");
                        registration.LoginName = Console.ReadLine();
                        while (!passwordMatch)
                        {
                            Console.WriteLine("Enter Password: ");
                            registration.Password = Console.ReadLine();
                            Console.WriteLine("Confirm Password: ");
                            registration.ConfirmPassword = Console.ReadLine();
                            passwordMatch = registration.Password == registration.ConfirmPassword;
                        }
                        _userService.Register(registration);
                        validInput = true;
                        break;
                    default:
                        Console.WriteLine("Invalid Input!");
                        break;
                }
            }

            //_userService.ReadAll();
        }

        public void UserOptions()
        {
            Console.WriteLine("\n\nSelect Transaction Type:");
            Console.WriteLine("1. Deposit");
            Console.WriteLine("2. Withdraw");
            Console.WriteLine("3. Transfer");

            bool validInput = false;
            ConsoleKeyInfo level = Console.ReadKey(true);
            while (!validInput)
            {
                switch (level.KeyChar)
                {
                    case (char)ConsoleKey.D1:
                        _transactService.DepositWithdraw(_user.AccountNumber, 12312, true);
                        validInput = true;
                        break;
                    case (char)ConsoleKey.D2:
                        _transactService.DepositWithdraw(_user.AccountNumber, 12312, false);
                        validInput = true;
                        break;
                    case (char)ConsoleKey.D3:
                        string recipientAccountNumber = string.Empty;
                        decimal amount = 0;
                        bool validAccountNumber = false;
                        while (!validAccountNumber)
                        {
                            Console.WriteLine("Enter Recipient Account Number: ");
                            string input = Console.ReadLine();
                            if (input.All(Char.IsDigit) && input.Length == 12)
                            {
                                validAccountNumber = true;
                                recipientAccountNumber = input;
                            }
                        }
                        bool validAmount = false;
                        while (!validAmount)
                        {
                            Console.WriteLine("Enter Amount to transfer: ");
                            string input = Console.ReadLine();
                            if (input.All(Char.IsDigit) && int.Parse(input) > 0)
                            {
                                validAmount = true;
                                amount = int.Parse(input);
                            }
                        }
                        _transactService.TransferBalance(_user.AccountNumber, recipientAccountNumber, amount);
                        validInput = true;
                        break;
                    default:
                        Console.WriteLine("Invalid Input!");
                        break;
                }
            }
           
        }
    }
}
