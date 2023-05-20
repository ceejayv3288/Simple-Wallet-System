using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Simple_Wallet_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Wallet_System
{
    public class Startup
    {
        public Startup()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .CreateLogger();

            Log.Logger.Information("Application Starting");
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IUserService, UserService>();
                    services.AddTransient<IStartupService, StartupService>();
                })
                .UseSerilog()
                .Build();

            var svc = ActivatorUtilities.CreateInstance<UserService>(host.Services);
            svc.ReadAll();
            Registration registration = new Registration
            {
                LoginName = "asdasdas",
                AccountNumber = "123123123123",
                Password = "123123123",
                Balance = 12312312,
                RegisterDate = DateTime.Now
            };
            Login user = new Login
            {
                LoginName = "CeeJay",
                Password = "password123"
            };
            //svc.Register(registration);
            svc.LoginUser(user);
        }

        static void BuildConfig(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false);        }
    }
}
