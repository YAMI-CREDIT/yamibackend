public interface IEmailSender
{
    Task SendEmail(string email, string subject, string htmlContent);
}