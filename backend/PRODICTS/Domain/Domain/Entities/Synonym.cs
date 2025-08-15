using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class Synonym
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("wordId")]
    public string WordId { get; set; } = string.Empty;

    [BsonElement("synonymWordId")]
    public string SynonymWordId { get; set; } = string.Empty;

    [BsonElement("synonymText")]
    public string SynonymText { get; set; } = string.Empty;

    [BsonElement("translation")]
    public string Translation { get; set; } = string.Empty;

    [BsonElement("exampleSentence")]
    public string? ExampleSentence { get; set; }

    [BsonElement("exampleTranslation")]
    public string? ExampleTranslation { get; set; }

    [BsonElement("similarityLevel")]
    public int SimilarityLevel { get; set; } = 1; // 1-5 (5 en yakın)

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
