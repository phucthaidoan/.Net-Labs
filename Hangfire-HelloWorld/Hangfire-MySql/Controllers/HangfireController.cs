using System;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Hangfire_MySql.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HangfireController : ControllerBase
    {
        [HttpPost]
        [Route("welcome")]
        public IActionResult Welcome(string userName)
        {
            var jobId = BackgroundJob.Enqueue(() => SendWelcomeMail(userName));
            return Ok($"Job Id {jobId} Completed. Welcome Mail Sent!");
        }

        public void SendWelcomeMail(string userName)
        {
            //Logic to Mail the user
            Console.WriteLine($"Welcome to our application, {userName}");
        }

        [HttpPost]
        [Route("invoice")]
        public IActionResult Invoice(string userName)
        {
            RecurringJob.AddOrUpdate("my-id", () => SendInvoiceMail(userName), Cron.Monthly);
            return Ok($"Recurring Job Scheduled. Invoice will be mailed Monthly for {userName}!");
        }

        [HttpPost]
        [Route("update")]
        public IActionResult UpdateInvoice(string userName)
        {
            RecurringJob.AddOrUpdate("my-id", () => SendInvoiceMail(userName), Cron.Daily, TimeZoneInfo.Local);
            return Ok($"Recurring Job Scheduled. Invoice will be mailed Monthly for {userName}!");
        }

        public void SendInvoiceMail(string userName)
        {
            //Logic to Mail the user
            Console.WriteLine($"Here is your invoice, {userName}");
        }
    }
}
