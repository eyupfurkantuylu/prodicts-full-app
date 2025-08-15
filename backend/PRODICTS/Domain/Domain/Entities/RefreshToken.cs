using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class RefreshToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("jwtId")]
    public string JwtId { get; set; } = string.Empty; // JWT'nin unique ID'si

    [BsonElement("deviceId")]
    public string? DeviceId { get; set; } // Hangi cihazdan geldiği

    [BsonElement("isUsed")]
    public bool IsUsed { get; set; } = false;

    [BsonElement("isRevoked")]
    public bool IsRevoked { get; set; } = false;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30); // 30 gün geçerli

    [BsonElement("usedAt")]
    public DateTime? UsedAt { get; set; }

    [BsonElement("revokedAt")]
    public DateTime? RevokedAt { get; set; }

    [BsonElement("revokedByIp")]
    public string? RevokedByIp { get; set; }

    [BsonElement("replacedByToken")]
    public string? ReplacedByToken { get; set; } // Yeni token ile değiştirildiğinde

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsUsed && !IsExpired;
}
