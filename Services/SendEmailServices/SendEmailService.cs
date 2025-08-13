using SawirahMunicipalityWeb.Models;
using SawirahMunicipalityWeb.Services.SendEmailServices;
using System.Net;
using System.Net.Mail;

public class SendEmailService : ISendEmailService
{
    public async Task SendEmailAscync(SendEmailDto sendEmailDto)
    {
        var mail = "mahmoudsmaily07@gmail.com"; // municipal account
        var pass = "dfdnvlcebyxtrehw";   // NEVER store in plain text in real projects

        using var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(mail, pass)
        };

        var messageBody = $"Name: {sendEmailDto.Name}\n\nMessage:\n{sendEmailDto.Body}";

        var message = new MailMessage
        {
            From = new MailAddress(mail),
            Subject = sendEmailDto.Subject,
            Body = messageBody,
            IsBodyHtml = false
        };

        message.To.Add(mail);
        await client.SendMailAsync(message);
    }
}
