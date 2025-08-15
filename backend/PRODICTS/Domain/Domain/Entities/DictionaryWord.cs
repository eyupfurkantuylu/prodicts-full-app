using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class DictionaryWord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("word")]
    public string Word { get; set; } = string.Empty;

    [BsonElement("language")]
    public string Language { get; set; } = string.Empty; // EN, TR

    [BsonElement("wordType")]
    public string WordType { get; set; } = string.Empty; // Noun, Verb, etc.

    [BsonElement("languageLevel")]
    public string LanguageLevel { get; set; } = string.Empty; // A1, A2, B1, etc.

    [BsonElement("pronunciation")]
    public string? Pronunciation { get; set; } // IPA notation

    [BsonElement("audioUrl")]
    public string? AudioUrl { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
