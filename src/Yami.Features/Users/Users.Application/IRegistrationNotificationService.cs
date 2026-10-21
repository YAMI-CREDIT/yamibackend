public interface IRegistrationNotificationService
{
    Task SendRegistrationSuccessfulSms(string phone);
}
