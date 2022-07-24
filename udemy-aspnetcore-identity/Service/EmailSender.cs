using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace udemy_aspnetcore_identity.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly IOptions<SmtpOptions> _options;
        public EmailSender(IOptions<SmtpOptions> options)
        {
            _options = options;
        }
        public async Task SendEmailAsync(string fromAddress, string toAddress, string subject, string message)
        {
            var mailMessage = new MailMessage(fromAddress, toAddress, subject, message);

            using (var client = new SmtpClient(_options.Value.Host, _options.Value.Port)
            {
                Credentials = new NetworkCredential(_options.Value.Username, _options.Value.Password),
                EnableSsl=true
                
            })
            {
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
