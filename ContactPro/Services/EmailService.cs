using ContactPro.Models;
using ContactPro.Services.Interfaces;
using ContactPro.Models.ViewModels;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace ContactPro.Services
{

    public class EmailService : IABEmailService
    {
        private readonly MailSettings _mailSettings;
        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public Task SendEmailAsync(AppUser appUser, List<Contact> contacts, EmailData emailData)
        {
            throw new NotImplementedException();
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
                var host = _mailSettings.Host ?? Environment.GetEnvironmentVariable("Host");
                var port = _mailSettings.Port != 0 ? _mailSettings.Port : int.Parse(Environment.GetEnvironmentVariable("Port")!);
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSender, _mailSettings.Password ?? Environment.GetEnvironmentVariable("Password"));

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
