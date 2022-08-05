using ContactPro.Models;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace ContactPro.Services.Interfaces
{
    public interface IABEmailService:IEmailSender
    {
        Task SendEmailAsync(AppUser appUser, List<Contact> contacts, EmailData emailData);
    }
}
