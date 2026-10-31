// The combined view of a user's agreements: what they are owed (assets, as
// creditor) and what they owe (debts, as debtor). Hosted on the Agreements
// feature because that is where the data is owned.
public class GetUserAgreementsResponse
{
    public IReadOnlyList<Agreement> debts { get; set; } = new List<Agreement>();
    public IReadOnlyList<Agreement> assets { get; set; } = new List<Agreement>();
}
