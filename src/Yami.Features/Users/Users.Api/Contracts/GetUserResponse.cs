public class GetUserResponse
{   
    public string name { get; set; }
    public IReadOnlyList<Agreement> agreements { get; set; } = new List<Agreement>();
}