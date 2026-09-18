// This class represents one row in the "Users" table in the database
public class User
{
    public Guid id { get; set; }
    public string phone { get; set; }
    public string name { get; set; }
    public string userSubId { get; set; } // This is the Cognito user sub, which is a unique identifier for the user in Cognito
    public string? email { get; set; }
    public string? area { get; set; }
    public string? identityType { get; set; }
    public string? identityNumber { get; set; }
    public string? userType { get; set; }
    public string? dateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
}