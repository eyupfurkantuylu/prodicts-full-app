using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Collocation
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("wordId")]
    public string WordId { get; set; } = string.Empty;

    [BsonElement("collocation")]
    public string CollocationText { get; set; } = string.Empty;

    [BsonElement("translation")]
    public string Translation { get; set; } = string.Empty;

    [BsonElement("exampleSentence")]
    public string? ExampleSentence { get; set; }

    [BsonElement("exampleTranslation")]
    public string? ExampleTranslation { get; set; }

    [BsonElement("language")]
    public string Language { get; set; } = string.Empty; // EN, TR

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
