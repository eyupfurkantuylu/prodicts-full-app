namespace Application.Models.DTOs;

public class SyncUserDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string? UserId { get; set; } // null = anonymous
    public DateTime LastSyncAt { get; set; }
    public UserSyncData LocalData { get; set; } = new();
    public bool ForceSync { get; set; } = false;
}

public class UserSyncData
{
    // Critical data that needs cloud backup
    public List<string> FavoriteWords { get; set; } = new();
    public Dictionary<string, object> UserPreferences { get; set; } = new();
    public List<StudySessionDto> StudySessions { get; set; } = new();
    public int TotalWordsLearned { get; set; }
    
    // Local-only data indicators
    public DateTime LastLocalActivity { get; set; }
    public int LocalSessionCount { get; set; }
}

public class StudySessionDto
{
    public DateTime Date { get; set; }
    public int WordsStudied { get; set; }
    public int CorrectAnswers { get; set; }
    public TimeSpan StudyDuration { get; set; }
}
