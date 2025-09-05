using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Domain.Entities;

namespace Persistence.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }
    public IMongoCollection<AppConfig> AppConfig => _database.GetCollection<AppConfig>("appconfigs");
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<AnonymousUser> AnonymousUsers => _database.GetCollection<AnonymousUser>("anonymoususers");
    public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("refreshtokens");
    public IMongoCollection<DictionaryWord> Words => _database.GetCollection<DictionaryWord>("words");
    public IMongoCollection<FlashCard> FlashCards => _database.GetCollection<FlashCard>("flashcards");
    public IMongoCollection<FlashCardGroup> FlashCardGroups => _database.GetCollection<FlashCardGroup>("flashcardgroups");
    public IMongoCollection<SubscriptionPlan> SubscriptionPlans => _database.GetCollection<SubscriptionPlan>("subscriptionplans");
    public IMongoCollection<SubscriptionWebhook> SubscriptionWebhooks => _database.GetCollection<SubscriptionWebhook>("subscriptionwebhooks");
    public IMongoCollection<UserProvider> UserProviders => _database.GetCollection<UserProvider>("userproviders");
    public IMongoCollection<WordTranslation> WordTranslations => _database.GetCollection<WordTranslation>("wordtranslations");
    public IMongoCollection<Synonym> Synonyms => _database.GetCollection<Synonym>("synonyms");
    public IMongoCollection<Antonym> Antonyms => _database.GetCollection<Antonym>("antonyms");
    public IMongoCollection<Collocation> Collocations => _database.GetCollection<Collocation>("collocations");
    public IMongoCollection<ExampleSentence> ExampleSentences => _database.GetCollection<ExampleSentence>("examplesentences");
    public IMongoCollection<PodcastSeries> PodcastSeries => _database.GetCollection<PodcastSeries>("podcastseries");
    public IMongoCollection<PodcastSeason> PodcastSeasons => _database.GetCollection<PodcastSeason>("podcastseasons");
    public IMongoCollection<PodcastEpisode> PodcastEpisodes => _database.GetCollection<PodcastEpisode>("podcastepisodes");
    public IMongoCollection<PodcastQuiz> PodcastQuizzes => _database.GetCollection<PodcastQuiz>("podcastquizzes");
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
