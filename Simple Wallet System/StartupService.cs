using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Wallet_System
{
    public class StartupService : IStartupService
    {
        public StartupService()
        {
            //PrintOptions();
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
                        validInput = true;
                        break;
                    case (char)ConsoleKey.D2:
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
