using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class SubscriptionPlan
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty; // Free, Pro, Pro+

    [BsonElement("providerEntitlementId")]
    public string? ProviderEntitlementId { get; set; } // Provider entitlement/product ID

    [BsonElement("maxWordsPerGroup")]
    public int? MaxWordsPerGroup { get; set; } // null = unlimited

    [BsonElement("maxGroups")]
    public int? MaxGroups { get; set; } // null = unlimited

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
