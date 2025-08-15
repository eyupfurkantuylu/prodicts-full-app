using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Antonym
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("wordId")]
    public string WordId { get; set; } = string.Empty;

    [BsonElement("antonymWordId")]
    public string AntonymWordId { get; set; } = string.Empty;

    [BsonElement("antonymText")]
    public string AntonymText { get; set; } = string.Empty;

    [BsonElement("translation")]
    public string Translation { get; set; } = string.Empty;

    [BsonElement("exampleSentence")]
    public string? ExampleSentence { get; set; }

    [BsonElement("exampleTranslation")]
    public string? ExampleTranslation { get; set; }

    [BsonElement("oppositionLevel")]
    public int OppositionLevel { get; set; } = 1; // 1-5 (5 en zıt)

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
