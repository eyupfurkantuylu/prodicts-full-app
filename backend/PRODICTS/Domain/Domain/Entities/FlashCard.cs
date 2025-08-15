using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class FlashCard
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("groupId")]
    public string GroupId { get; set; } = string.Empty;

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("sourceWord")]
    public string SourceWord { get; set; } = string.Empty;

    [BsonElement("targetWord")]
    public string TargetWord { get; set; } = string.Empty;

    [BsonElement("currentStep")]
    public int CurrentStep { get; set; } = 0; // 0-6 (0=yeni, 1-6=tekrar aşamaları)

    [BsonElement("nextReviewDate")]
    public DateTime NextReviewDate { get; set; }

    [BsonElement("firstLearningDate")]
    public DateTime? FirstLearningDate { get; set; }

    [BsonElement("reviewDates")]
    public List<DateTime> ReviewDates { get; set; } = new();

    [BsonElement("isCompleted")]
    public bool IsCompleted { get; set; } = false;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
