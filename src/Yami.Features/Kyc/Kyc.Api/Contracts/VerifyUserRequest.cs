public class VerifyUserRequest
{
    required public string identityType { get; set; }   // "nin" | "bvn"
    required public string identityNumber { get; set; }
}
