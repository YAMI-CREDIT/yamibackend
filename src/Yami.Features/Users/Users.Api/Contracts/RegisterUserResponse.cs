public class RegisterUserResponse
{   
    public bool userCreated { get; set; }
    public bool? businessCreated { get; set; }
    public Guid userId { get; set; }
    public Guid? businessId { get; set; }
}