using ContactPro.Models;
using ContactPro.Services.Interfaces;
using ContactPro.Models.ViewModels;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ContactPro.Services
{

    public class EmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSender = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email");

            //add all email addresses to the "TO" for the email
            MimeMessage newEmail = new();
            newEmail.Sender = MailboxAddress.Parse(emailSender);

            foreach(var emailAddress in email.Split(';'))
            {
                newEmail.To.Add(MailboxAddress.Parse(emailAddress));
            }
            //add the subject for the email
            newEmail.Subject = subject;

            //add the body for the email
            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;
            newEmail.Body = emailBody.ToMessageBody();

            //send the email
            using SmtpClient smtpClient = new();
            try
            {
                var host = _mailSettings.MailHost ?? Environment.GetEnvironmentVariable("MailHost");
                var port = _mailSettings.MailPort != 0 ? _mailSettings.MailPort : int.Parse(Environment.GetEnvironmentVariable("MailPort")!);
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSender, _mailSettings.MailPassword ?? Environment.GetEnvironmentVariable("MailPassword"));

                await smtpClient.SendAsync(newEmail);
                await smtpClient.DisconnectAsync(true);
            }
            catch
            {
                throw;
            }
        }
    }
}
