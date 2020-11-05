using System;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace DependencyInjection_ConsoleApp
{
    public interface IEmailSender
    {
        void SendWelcomeMail(string userName);
    }

    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public void SendWelcomeMail(string userName)
        {
            //Logic to Mail the user
            Console.WriteLine($"Welcome to our application, {userName}");
            _logger.LogInformation($"Welcome to our application, {userName}");
        }
    }
}
