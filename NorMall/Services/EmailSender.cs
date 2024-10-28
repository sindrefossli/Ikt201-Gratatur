namespace NorMall.Services;

using SendGrid;
using SendGrid.Helpers.Mail;



internal sealed class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {

        var apiKey = "";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("yoho1423@gmail.com", "NorMall");
        var to = new EmailAddress(email, "Customer");
        var plainTextContent = "Confirm email address";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlMessage);

        var response = client.SendEmailAsync(msg);
        
        return response;
    }
}
