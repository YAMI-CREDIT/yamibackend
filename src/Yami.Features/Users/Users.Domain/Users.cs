// This class represents one row in the "Users" table in the database
public class User
{
    public Guid id { get; set; }
    public string phone { get; set; }
    public string name { get; set; }
    public string userType { get; set; }
    public string dateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
}