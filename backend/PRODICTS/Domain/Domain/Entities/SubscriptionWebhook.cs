using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class SubscriptionWebhook
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("provider")]
    public string Provider { get; set; } = string.Empty; // RevenueCat, Adapty, Direct

    [BsonElement("eventType")]
    public string EventType { get; set; } = string.Empty; // purchase, renewal, cancellation, etc.

    [BsonElement("userId")]
    public string? UserId { get; set; } // Bizim user ID'miz

    [BsonElement("providerUserId")]
    public string? ProviderUserId { get; set; } // Provider'daki user ID

    [BsonElement("entitlementId")]
    public string? EntitlementId { get; set; }

    [BsonElement("productId")]
    public string? ProductId { get; set; }

    [BsonElement("rawPayload")]
    public string RawPayload { get; set; } = string.Empty; // Gelen webhook data

    [BsonElement("isProcessed")]
    public bool IsProcessed { get; set; } = false;

    [BsonElement("processedAt")]
    public DateTime? ProcessedAt { get; set; }

    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
