using System.ComponentModel.DataAnnotations;

/// <summary>Business details collected during onboarding, after signup.</summary>
public class OnboardingRequest
{
    [Required]
    public string BusinessName { get; set; } = string.Empty;

    public string? BusinessType { get; set; }
    public string? Industry { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? BusinessAddress { get; set; }

    [EmailAddress]
    public string? BusinessEmail { get; set; }

    [Phone]
    public string? BusinessPhone { get; set; }

    public string? YearsInOperation { get; set; }
    public string? EstimatedMonthlyRevenue { get; set; }
}

public class OnboardingResponse
{
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

    public static OnboardingResponse From(BusinessProfile profile) => new()
    {
        BusinessName = profile.BusinessName,
        BusinessType = profile.BusinessType,
        Industry = profile.Industry,
        RegistrationNumber = profile.RegistrationNumber,
        BusinessAddress = profile.BusinessAddress,
        BusinessEmail = profile.BusinessEmail,
        BusinessPhone = profile.BusinessPhone,
        YearsInOperation = profile.YearsInOperation,
        EstimatedMonthlyRevenue = profile.EstimatedMonthlyRevenue,
        IsCompleted = profile.IsCompleted,
        CreatedAt = profile.CreatedAt,
        UpdatedAt = profile.UpdatedAt
    };
}
