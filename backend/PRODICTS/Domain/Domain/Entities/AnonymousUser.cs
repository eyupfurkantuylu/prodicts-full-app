using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class AnonymousUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [BsonElement("deviceType")]
    public string DeviceType { get; set; } = string.Empty; // iOS, Android, Web

    [BsonElement("appVersion")]
    public string AppVersion { get; set; } = string.Empty;

    [BsonElement("lastSyncAt")]
    public DateTime LastSyncAt { get; set; } = DateTime.UtcNow;

    [BsonElement("syncData")]
    public AnonymousUserSyncData SyncData { get; set; } = new();

    [BsonElement("isUpgraded")]
    public bool IsUpgraded { get; set; } = false;

    [BsonElement("upgradedUserId")]
    public string? UpgradedUserId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastActiveAt")]
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
    
    [BsonElement("role")] 
    public string Role { get; set; } = "Anonymous";
}

public class AnonymousUserSyncData
{
    [BsonElement("favoriteWords")]
    public List<string> FavoriteWords { get; set; } = new();

    [BsonElement("userPreferences")]
    public Dictionary<string, object> UserPreferences { get; set; } = new();

    [BsonElement("studySessions")]
    public List<StudySession> StudySessions { get; set; } = new();

    [BsonElement("totalWordsLearned")]
    public int TotalWordsLearned { get; set; } = 0;

    [BsonElement("currentStreak")]
    public int CurrentStreak { get; set; } = 0;

    [BsonElement("longestStreak")]
    public int LongestStreak { get; set; } = 0;

    [BsonElement("totalStudyTime")]
    public TimeSpan TotalStudyTime { get; set; } = TimeSpan.Zero;
}

public class StudySession
{
    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("wordsStudied")]
    public int WordsStudied { get; set; }

    [BsonElement("correctAnswers")]
    public int CorrectAnswers { get; set; }

    [BsonElement("studyDuration")]
    public TimeSpan StudyDuration { get; set; }

    [BsonElement("studyType")]
    public string StudyType { get; set; } = string.Empty; // FlashCard, Quiz, Reading
}
