using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class WordTranslation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("sourceWordId")]
    public string SourceWordId { get; set; } = string.Empty;

    [BsonElement("targetWordId")]
    public string TargetWordId { get; set; } = string.Empty;

    [BsonElement("translation")]
    public string Translation { get; set; } = string.Empty; // Ana çeviri

    [BsonElement("definition")]
    public string? Definition { get; set; } // Sözlük karşılığı

    [BsonElement("detailedTranslation")]
    public string? DetailedTranslation { get; set; } // Detaylı çeviri

    [BsonElement("subMeanings")]
    public List<string> SubMeanings { get; set; } = new(); // Alt anlamlar

    [BsonElement("isMainTranslation")]
    public bool IsMainTranslation { get; set; } = true;

    [BsonElement("usageFrequency")]
    public int UsageFrequency { get; set; } = 0; // Kullanım sıklığı

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
