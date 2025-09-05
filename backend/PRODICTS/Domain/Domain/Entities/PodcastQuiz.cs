using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class PodcastQuiz
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("podcastEpisodeId")]
    public string PodcastEpisodeId { get; set; }
    
    [BsonElement("question")]
    public string Question { get; set; } = null!;
    
    [BsonElement("answers")]
    public string Answers { get; set; } = null!; 
    
    [BsonElement("correctAnswerIndex")]
    public int CorrectAnswerIndex { get; set; }
    
    
}