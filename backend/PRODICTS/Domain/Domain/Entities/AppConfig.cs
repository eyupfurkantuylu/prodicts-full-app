using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class AppConfig
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("appName")]
    public string AppName { get; set; } = string.Empty;
    
    [BsonElement("iosPackageName")]
    public string IosPackageName { get; set; } = string.Empty;
    
    [BsonElement("iosVersion")]
    public string IosVersion { get; set; } = string.Empty;
    
    [BsonElement("iosBuildNumber")]
    public string IosBuildNumber { get; set; } = string.Empty;
    
    [BsonElement("androidPackageName")]
    public string AndroidPackageName { get; set; } = string.Empty;
    
    [BsonElement("androidVersion")]
    public string AndroidVersion { get; set; } = string.Empty;
    
    [BsonElement("androidBuildNumber")]
    public string AndroidBuildNumber { get; set; } = string.Empty;
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    
    
    
}