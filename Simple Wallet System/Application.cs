using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Simple_Wallet_System.Models;
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

        public Application(ILogger<UserService> log, IConfiguration config, IUserService userService)
        {
            _logger = log;
            _config = config;
            _userService = userService;
        }

        public void PrintOptions()
        {
            bool validInput = false;
            Console.WriteLine("Simple Wallet System\nChoose from the options below:\n");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");

            while (!validInput)
            {
                ConsoleKeyInfo level = Console.ReadKey(true);
                switch (level.KeyChar)
                {
                    case (char)ConsoleKey.D1:
                        Login login = new Login();
                        Console.WriteLine("Enter Username: ");
                        login.LoginName = Console.ReadLine();
                        Console.WriteLine("Enter Password: ");
                        login.Password = Console.ReadLine();
                        User user = _userService.LoginUser(login);
                        validInput = true;
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
    }
}
