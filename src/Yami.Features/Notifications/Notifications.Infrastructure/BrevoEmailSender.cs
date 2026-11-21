using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string? ApiKey { get; set; }
    public string Endpoint { get; set; } = "https://api.brevo.com/v3/smtp/email";
    public string SenderName { get; set; } = "Yami Credit";
    public string SenderEmail { get; set; } = "yamicredit@gmail.com";
    public string AdminEmail { get; set; } = string.Empty;
}

public class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly EmailOptions _options;
    private readonly ILogger<BrevoEmailSender> _logger;

    public BrevoEmailSender(
        HttpClient httpClient,
        IOptions<EmailOptions> options,
        ILogger<BrevoEmailSender> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task SendEmail(string email, string subject, string htmlContent)
        => SendEmailWithAttachment(email, subject, htmlContent, Array.Empty<Attachment>());

    public async Task SendEmailWithAttachment(
        string email,
        string subject,
        string htmlContent,
        IEnumerable<Attachment> attachments)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "Email API key is missing. Set Email:ApiKey in configuration or Environment.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.TryAddWithoutValidation("api-key", _options.ApiKey);
        var sender = new { name = _options.SenderName, email = _options.SenderEmail };
        var recipients = new[] { new { email } };
        var attachmentPayload = attachments
            .Select(attachment => new
            {
                name = attachment.Filename,
                content = Convert.ToBase64String(attachment.Content),
                contentType = attachment.ContentType,
            })
            .ToArray();
        var payload = new { sender, to = recipients, subject, htmlContent, attachment = attachmentPayload };

        request.Content = JsonContent.Create(payload);

        using var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation(
            "brevo_email_response: StatusCode {StatusCode}, Response {Response}",
            (int)response.StatusCode,
            responseBody);

        response.EnsureSuccessStatusCode();
    }
}