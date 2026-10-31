using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

public class ResendEmailSender : IEmailSender
{
    private readonly IResend _resend;
    private readonly EmailOptions _options;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(
        IResend resend,
        IOptions<EmailOptions> options,
        ILogger<ResendEmailSender> logger)
    {
        _resend = resend;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmail(string email, string subject, string htmlContent)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException(
                    "Email API key is missing. Set Email:ApiKey in configuration or Environment.");
            }

            var message = new EmailMessage
            {
                From = $"{_options.SenderName} <{_options.SenderEmail}>",
                Subject = subject,
                HtmlBody = htmlContent,
            };
            message.To.Add(email);

            var response = await _resend.EmailSendAsync(message);
            _logger.LogInformation(
                "resend_email_response: EmailId {EmailId}",
                response.Content);
        }
        catch (ResendException exception)
        {
            _logger.LogError(
                exception,
                "resend_email_failed: StatusCode {StatusCode}, ErrorType {ErrorType}, Message {ErrorMessage}",
                exception.StatusCode,
                exception.ErrorType,
                exception.Message);
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "resend_email_failed");
            throw;
        }

    }
}