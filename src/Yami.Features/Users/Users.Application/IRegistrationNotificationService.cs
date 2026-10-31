public interface IRegistrationNotificationService
{
    Task SendRegistrationSuccessfulSms(string phone, string userName);
    Task SendRegistrationSuccessfulEmail(string email, string userName);
}
