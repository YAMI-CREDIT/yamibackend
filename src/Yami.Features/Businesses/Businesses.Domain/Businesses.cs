public class Business
{
    public Guid id { get; set; }
    required public string name { get; set; }
    public Guid userId { get; set; }
    public string? registrationNo { get; set; }
    required public string area { get; set; }
    public DateTime createdAt { get; set; }
    public bool verified { get; set; }

}