using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class ExampleSentence
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("wordId")]
    public string WordId { get; set; } = string.Empty;

    [BsonElement("sentence")]
    public string Sentence { get; set; } = string.Empty;

    [BsonElement("translation")]
    public string Translation { get; set; } = string.Empty;

    [BsonElement("language")]
    public string Language { get; set; } = string.Empty; // EN, TR

    [BsonElement("difficulty")]
    public string? Difficulty { get; set; } // A1, A2, etc.

    [BsonElement("audioUrl")]
    public string? AudioUrl { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
