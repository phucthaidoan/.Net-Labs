using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Hangfire;
using Hangfire.Logging;
using Hangfire.Storage.MySql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DependencyInjection_ConsoleApp
{
    public class App
    {
        private readonly IConfiguration _config;
        private readonly IUser _user;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<App> _logger;

        public App(IConfiguration config, IUser user, IEmailSender emailSender, ILogger<App> logger)
        {
            _config = config;
            _user = user;
            _emailSender = emailSender;
            _logger = logger;
        }

        public void Run()
        {
            /*var logDirectory = _config.GetValue<string>("Runtime:LogOutputDirectory");
            // Using serilog here, can be anything
            var log = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(logDirectory)
                .CreateLogger();

            log.Information("Serilog logger information");*/
            _logger.LogInformation("Serilog logger information");

            StartHangfire();
            //_user.TruncateName("Jerry     ");
            //log.Information($"User name: {_user.Name}");

            //Console.WriteLine("Hello from App.cs");
        }

        private void SetJobSchedule()
        {
            RecurringJob.AddOrUpdate<IEmailSender>(
                "email-sender-id", 
                sender => sender.SendWelcomeMail("hello"),
                Cron.Minutely);
        }

        private void StartHangfire()
        {
            var options =
                new MySqlStorageOptions
                {
                    TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    PrepareSchemaIfNecessary = true,
                    DashboardJobListLimit = 50000,
                    TransactionTimeout = TimeSpan.FromMinutes(1),
                    TablesPrefix = "Hangfire"
                };
            var connectionString = "server=127.0.0.1;uid=root;pwd=root;database=hangfire;Allow User Variables=True";
            var storage = new MySqlStorage(connectionString, options);

            GlobalConfiguration.Configuration.UseStorage(storage);
            SetJobSchedule();

            using (var server = new BackgroundJobServer())
            {
                Console.WriteLine("Hangfire Server started. Press any key to exit...");

                Console.ReadKey();
            }
        }
    }
}
