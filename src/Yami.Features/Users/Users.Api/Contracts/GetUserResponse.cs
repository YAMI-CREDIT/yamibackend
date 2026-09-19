public class GetUserResponse
{   
    public string name { get; set; }
    public IReadOnlyList<Agreement> debts { get; set; } = new List<Agreement>();
    public IReadOnlyList<Agreement> assets { get; set; } = new List<Agreement>();
}