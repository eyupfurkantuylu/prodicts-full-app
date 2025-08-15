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

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<AnonymousUser> AnonymousUsers => _database.GetCollection<AnonymousUser>("anonymoususers");
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
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
