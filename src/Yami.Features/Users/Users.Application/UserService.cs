using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly UsersDbContext _db;
    private readonly IInvitationService _invitationService;
    private readonly IRegistrationNotificationService _registrationNotificationService;

    public UserService(
        UsersDbContext db,
        IInvitationService invitationService,
        IRegistrationNotificationService registrationNotificationService)
    {
        _db = db;
        _invitationService = invitationService;
        _registrationNotificationService = registrationNotificationService;
    }

    // This is the actual Registration logic
    public async Task<User?> RegisterUser(
        string phone, string name, string userSubId, string? dateOfBirth,
        string? email, bool termsAccepted=true, string? area=null,
        string? identityType=null, string? identityNumber=null,
        string? userType=null)
    {
        bool alreadyExists = await _db.Users
            .AnyAsync(u => u.phone == phone);

        if (alreadyExists)
        {
            return null;   
        }

        // If this phone has a pending, unexpired invitation, reuse its pre-allocated
        // id so any agreement already referencing it stays valid. Otherwise the
        // user gets a fresh id.
        var pendingInvitation = await _db.Invitations
            .FirstOrDefaultAsync(i => i.phone == phone
                && i.status == InvitationStatus.Pending
                && i.expiresAt > DateTime.UtcNow);

        var newUser = new User
        {
            id = pendingInvitation?.id ?? Guid.NewGuid(),
            phone = phone,
            name = name,
            userSubId = userSubId,
            email = email,
            termsAccepted=termsAccepted,
            area = area,
            identityType = identityType,
            identityNumber = identityNumber,
            userType = userType,
            dateOfBirth = dateOfBirth,
            createdAt = DateTime.UtcNow,
            verified = false
        };

        _db.Users.Add(newUser);

        // Consume the invitation in the same transaction as the user insert.
        if (pendingInvitation is not null)
        {
            pendingInvitation.status = InvitationStatus.Accepted;
        }

        await _db.SaveChangesAsync();
        await _registrationNotificationService.SendRegistrationSuccessfulSms(phone);

        return newUser;
    }

    // Get user by database ID.
    public async Task<User?> GetUser(Guid id)
    {
        Console.WriteLine($"INFO: Getting user by ID: {id}");
        return await _db.Users.FindAsync(id);
    }

    // Get user by the Cognito auth claim "sub".
    public async Task<User?> GetUserBySub(string sub)
    {
        if (string.IsNullOrWhiteSpace(sub))
        {
            return null;
        }

        Console.WriteLine($"INFO: Getting user by Cognito sub: {sub}");
        return await _db.Users.FirstOrDefaultAsync(u => u.userSubId == sub);
    }

    // Get user by phone number.
    public async Task<User?> GetUserByPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        Console.WriteLine($"INFO: Getting user by phone: {phone}");
        return await _db.Users.FirstOrDefaultAsync(u => u.phone == phone);
    }

    // Returns the pending, unexpired invitation for this phone, if one exists.
    public async Task<Invitation?> GetPendingInvitationByPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        return await _db.Invitations
            .FirstOrDefaultAsync(i => i.phone == phone
                && i.status == InvitationStatus.Pending
                && i.expiresAt > DateTime.UtcNow);
    }

    // Flips the user's final verification outcome. Returns false if the user
    // doesn't exist. The Kyc module calls this with the provider's result.
    public async Task<bool> SetVerified(Guid id, bool verified)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
        {
            return false;
        }

        user.verified = verified;
        await _db.SaveChangesAsync();
        return true;
    }

    // Creates an invitation for a counterparty who is not yet registered.
    // The invitation's id is pre-allocated as the future user's id, so the agreement
    // can reference it immediately. Returns the existing invitation if one is already
    // pending for this phone (idempotent), or null if the phone is invalid.
    private async Task<Invitation?> CreateInvitation(
        string phone, Guid agreementId, Guid invitedBy, Guid preAllocatedId)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        Console.WriteLine($"INFO: Creating invitation for phone: {phone}");

        // Idempotency: reuse an existing invitation for this phone.
        var existing = await _db.Invitations
            .FirstOrDefaultAsync(i => i.phone == phone
                && i.status == InvitationStatus.Pending
                && i.expiresAt > DateTime.UtcNow);

        if (existing is not null)
        {
            return existing;
        }

        var invitation = new Invitation
        {
            id = preAllocatedId,
            phone = phone,
            agreementId = agreementId,
            invitedBy = invitedBy,
            createdAt = DateTime.UtcNow,
            expiresAt = DateTime.UtcNow.AddHours(72),
            status = InvitationStatus.Pending
        };

        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync();

        return invitation;
    }

    // Creates an agreement invitation and sends the counterparty an SMS with a link
    // to the generic registration page. No token in the link: the counterparty
    // authenticates via Cognito (phone OTP) and the normal registration flow matches
    // them to this invitation by phone. Returns null if the phone is invalid.
    public async Task<Invitation?> InviteToAgreement(
        string phone, Guid agreementId, Guid invitedBy, Guid preAllocatedId)
    {
        if (preAllocatedId == Guid.Empty)
        {
            return null;
        }

        var invitation = await CreateInvitation(phone, agreementId, invitedBy, preAllocatedId);

        if (invitation is null)
        {
            return null;
        }

        // Look up the inviter's name for the SMS message.
        var inviter = await _db.Users.FindAsync(invitedBy);
        var inviterName = inviter?.name ?? "a Yami user";

        await _invitationService.SendInvitationSms(phone, inviterName);

        return invitation;
    }

    // Invites a user by phone. No invitation row is
    // created: this is a pure notification. The invitee registers normally and
    // is matched to any pending agreement invitation by phone if one exists.
    public async Task<bool> InviteByPhone(string phone, Guid invitedBy)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return false;
        }

        Console.WriteLine($"INFO: Inviting user by phone: {phone}");

        var inviter = await _db.Users.FindAsync(invitedBy);
        var inviterName = inviter?.name ?? "a Yami user";

        await _invitationService.SendInvitationSms(phone, inviterName);

        return true;
    }

    // Invites a user by email. No invitation row is
    // created: this is a pure notification.
    public async Task<bool> InviteByEmail(string email, Guid invitedBy)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        Console.WriteLine($"INFO: Inviting user by email: {email}");

        var inviter = await _db.Users.FindAsync(invitedBy);
        var inviterName = inviter?.name ?? "a Yami user";

        await _invitationService.SendInvitationEmail(email, inviterName);

        return true;
    }
}