public interface IEmailSender
{
    Task SendAsync(string email, string subject, string message);
}
