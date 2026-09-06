using System.ComponentModel.DataAnnotations;

public class PhoneOtpRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class VerifyPhoneOtpRequest
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}
