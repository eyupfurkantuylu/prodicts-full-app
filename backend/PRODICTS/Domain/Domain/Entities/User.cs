using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("profilePictureUrl")]
    public string? ProfilePictureUrl { get; set; }

    [BsonElement("emailVerified")]
    public bool EmailVerified { get; set; } = false;

    [BsonElement("providers")]
    public List<UserProvider> Providers { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("subscriptionProvider")]
    public string? SubscriptionProvider { get; set; } // RevenueCat, Adapty, Direct

    [BsonElement("providerUserId")]
    public string? ProviderUserId { get; set; } // Provider'daki user ID

    [BsonElement("currentSubscriptionPlan")]
    public string CurrentSubscriptionPlan { get; set; } = "Free"; // Free, Pro, Pro+

    [BsonElement("subscriptionExpiresAt")]
    public DateTime? SubscriptionExpiresAt { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}
