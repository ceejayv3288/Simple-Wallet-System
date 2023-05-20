// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Simple_Wallet_System;
using Simple_Wallet_System.Models;
using Simple_Wallet_System.Services;
using System.Data.SqlClient;

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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IStartupService, StartupService>();
        services.AddScoped<ITransactService, TransactService>();
    })
    .UseSerilog()
    .Build();

var app = ActivatorUtilities.CreateInstance<Application>(host.Services);
app.PrintOptions();
//var userSvc = ActivatorUtilities.CreateInstance<UserService>(host.Services);
//var startSvc = ActivatorUtilities.CreateInstance<StartupService>(host.Services);
//userSvc.ReadAll();
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
//userSvc.Register(registration);
//userSvc.LoginUser(user);
//startSvc.PrintOptions();

static void BuildConfig(IConfigurationBuilder configurationBuilder)
{
    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false);
}