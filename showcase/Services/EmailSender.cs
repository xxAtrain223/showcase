using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace showcase.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly ApplicationSettings ApplicationSettings;
        private readonly SmtpClient SmtpClient;

        public EmailSender(ApplicationSettings applicationSettings)
        {
            ApplicationSettings = applicationSettings;

            SmtpServer smtpServer = ApplicationSettings.Contact.SmtpServer;
            SmtpClient = new SmtpClient
            {
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Host = smtpServer.Host,
                Port = (int)smtpServer.Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpServer.Username, smtpServer.Password)
            };
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(ApplicationSettings.Contact.Email, "DO NOT REPLY");
            mailMessage.ReplyToList.Add(new MailAddress(ApplicationSettings.Contact.Email, "DO NOT REPLY"));
            mailMessage.To.Add(new MailAddress(email));
            mailMessage.Subject = subject + " - DO NOT REPLY";
            mailMessage.Body = message;

            await SendEmailAsync(mailMessage);
        }

        public async Task SendEmailAsync(MailMessage message)
        {
            await SmtpClient.SendMailAsync(message);
        }

        public async Task SendEmailConfirmationAsync(string email, string link)
        {
            await SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }
    }
}
