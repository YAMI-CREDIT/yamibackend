using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Configuration for the SMS provider. Bound from the "Sms" section of
// appsettings.json (or environment variables such as Sms__ApiKey).
public class SmsOptions
{
    public const string SectionName = "Sms";

    // Provider API key. Required.
    public string? ApiKey { get; set; }

    // Provider endpoint. Defaults to Termii's send endpoint.
    public string Endpoint { get; set; } = "https://v3.api.termii.com/api/sms/send";

    // The "from" / sender name shown to the recipient.
    public string From { get; set; } = "FloathHub";
}

public class TermiiSmsSender : ISmsSender
{
    private static readonly HttpClient s_http = new() { Timeout = TimeSpan.FromSeconds(30) };

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly SmsOptions _options;
    private readonly ILogger<TermiiSmsSender> _logger;

    public TermiiSmsSender(IOptions<SmsOptions> options, ILogger<TermiiSmsSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SendSmsResult> SendSms(string phone, string message)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "SMS API key is missing. Set Sms:ApiKey in configuration.");
        }

        var normalizedPhone = NormalizePhone(phone);

        var payload = new
        {
            api_key = _options.ApiKey,
            to = normalizedPhone,
            from = _options.From,
            sms = message,
            type = "plain",
            channel = "generic",
        };

        var response = await s_http.PostAsync(
            _options.Endpoint,
            new StringContent(JsonSerializer.Serialize(payload, s_jsonOptions), Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();

        // Log the raw response for debugging purposes.
        _logger.LogInformation(
            "sms_provider_response: {Response}",
            body);

        var result = JsonSerializer.Deserialize<SendSmsResult>(body, s_jsonOptions);

        // Termii returns an error object even with HTTP 200 in some cases.
        if (result is null || (result.Code != "ok" && !string.Equals(result.Message, "success", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                result?.Message ?? "SMS sending failed");
        }

        return result;
    }

    // Strips whitespace, then rewrite Nigerian numbers to the 234 country-code form.
    private static string NormalizePhone(string phone)
    {
        var value = Regex.Replace(phone, @"\s+", "");
        value = Regex.Replace(value, @"^\+234", "234");
        value = Regex.Replace(value, @"^234", "234");
        value = Regex.Replace(value, @"^0", "234");
        return value;
    }
}
