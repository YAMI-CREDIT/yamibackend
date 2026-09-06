// The onboarding details collected after signup: the user's business information.
// One-to-one with User.
public class BusinessProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessType { get; set; }
    public string? Industry { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? BusinessAddress { get; set; }
    public string? BusinessEmail { get; set; }
    public string? BusinessPhone { get; set; }
    public string? YearsInOperation { get; set; }
    public string? EstimatedMonthlyRevenue { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
