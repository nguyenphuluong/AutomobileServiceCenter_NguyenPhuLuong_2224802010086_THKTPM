using MailKit.Net.Smtp;
using MimeKit;

namespace ASC.Web.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("ASC", "your-email@gmail.com"));
            mimeMessage.To.Add(MailboxAddress.Parse(email));
            mimeMessage.Subject = subject;

            mimeMessage.Body = new TextPart("html")
            {
                Text = message
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, false);
            await client.AuthenticateAsync("your-email@gmail.com", "your-app-password");
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }
    }
}