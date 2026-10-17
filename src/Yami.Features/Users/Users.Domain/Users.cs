// This class represents one row in the "Users" table in the database
public class User
{
    public Guid id { get; set; }
    required public string phone { get; set; }
    required public string name { get; set; }
    required public string userSubId { get; set; } // This is the Cognito user sub, which is a unique identifier for the user in Cognito
    public string? email { get; set; }
    public bool termsAccepted { get; set; }
    public string? area { get; set; }
    public string? identityType { get; set; }
    public string? identityNumber { get; set; }
    public string? userType { get; set; }
    public string? dateOfBirth { get; set; }
    public DateTime createdAt { get; set; }
    public bool verified { get; set; }
}

// The lifecycle of an invitation to a counterparty who is not yet registered.
public enum InvitationStatus
{
    Pending,   // Invitation sent, awaiting the counterparty to complete registration
    Accepted,  // Counterparty registered; a User row now exists with this id
    Expired,   // The invitation lapsed before it was used
    Revoked    // The inviter cancelled the invitation
}

// This class represents one row in the "Invitations" table in the database.
// It holds a pre-allocated user GUID for a counterparty who has not yet registered.
// The phone number is the only personal data stored, and the record is time boxed
// (expiresAt) so it can be deleted if left unused. No token is needed: the counterparty
// authenticates via Cognito (phone OTP) and the normal registration flow matches them
// to this invitation by phone.
public class Invitation
{
    public Guid id { get; set; } // Pre-allocated: becomes the user's id on registration
    required public string phone { get; set; }
    public Guid agreementId { get; set; } // The agreement this invitation was created for
    public Guid invitedBy { get; set; } // The registered user who created the invitation
    public DateTime createdAt { get; set; }
    public DateTime expiresAt { get; set; }
    public InvitationStatus status { get; set; } = InvitationStatus.Pending;
}