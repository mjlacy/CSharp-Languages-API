using CSharp_Languages_API.Models;

using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CSharp_Languages_API.Services;

public class LanguagesService {
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IMongoCollection<Language> _languagesCollection;

    public LanguagesService(IOptions<LanguagesDatabaseSettings> languagesDatabaseSettings) {
        MongoClient mongoClient = new MongoClient(languagesDatabaseSettings.Value.ConnectionString);

        _mongoDatabase = mongoClient.GetDatabase(languagesDatabaseSettings.Value.DatabaseName);

        _languagesCollection =
            _mongoDatabase.GetCollection<Language>(languagesDatabaseSettings.Value.LanguagesCollectionName);
    }

    public bool Ping() {
        // try {
        using (CancellationTokenSource timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(Constants.PingTimeoutMs))) {
            BsonDocument ping = _mongoDatabase.RunCommand<BsonDocument>(new BsonDocument("ping", 1), null, timeoutCancellationTokenSource.Token);
            return ping.GetValue("ok") == 1;
        }
        // }
        // catch (Exception ex) {
        //     // Console.WriteLine(ex.Message);
        //     return false;
        // }
    }

    public Languages Find(Language language) {
        BsonDocument filters = new BsonDocument();
        
        if (!string.IsNullOrEmpty(language.Name)) {
            filters["name"] = language.Name;
        }
        
        if (language.Creators != null && language.Creators.Length > 0) {
            filters["creators"] = new BsonDocument("$all", new BsonArray(language.Creators));
        }
        
        if (language.Extensions != null && language.Extensions.Length > 0) {
            filters["extensions"] = new BsonDocument("$all", new BsonArray(language.Extensions));
        }
        
        if (language.FirstAppeared != null) {
            filters["firstAppeared"] = language.FirstAppeared;
        }
        
        if (language.Year is not null && language.Year != 0) {
            filters["year"] = language.Year;
        }
        
        if (!string.IsNullOrEmpty(language.Wiki)) {
            filters["wiki"] = language.Wiki;
        }

        return new Languages(_languagesCollection.Find(filters).ToList());
    }

    public Language? FindOne(string id) {
        ObjectId objectId;
        if (!ObjectId.TryParse(id, out objectId)) {
            return new Language{Id = ResponseCode.InvalidId.ToString()};
        }

        return _languagesCollection.Find(lang => lang.Id == id).FirstOrDefault();
    }

    public string InsertOne(Language language) {
        language.Id = ObjectId.GenerateNewId().ToString();
        
        _languagesCollection.InsertOne(language);
        
        return language.Id;
    }

    public string ReplaceOne(string id, Language language) {
        ObjectId objectId;
        if (!ObjectId.TryParse(id, out objectId)) {
            return ResponseCode.InvalidId.ToString();
        } else {
            language.Id = objectId.ToString();
        }
        
        ReplaceOptions upsert = new ReplaceOptions { IsUpsert = true };
        
        ReplaceOneResult ror = _languagesCollection.ReplaceOne(lang => lang.Id == id, language, upsert);

        return ror.UpsertedId?.ToString() ?? ResponseCode.Found.ToString();
    }

    public ResponseCode UpdateOne(string id, Object language) {
        ObjectId objectId;
        if (!ObjectId.TryParse(id, out objectId)) {
            return ResponseCode.InvalidId;
        }

        UpdateResult uor = _languagesCollection.UpdateOne(
            lang => lang.Id == id,
            new BsonDocument {{"$set", BsonDocument.Parse(language.ToString())}}
        );

        return uor.MatchedCount == 1 ? ResponseCode.Found : ResponseCode.NotFound;
    }


    public ResponseCode DeleteOne(string id) {
        ObjectId objectId;
        if (!ObjectId.TryParse(id, out objectId)) {
            return ResponseCode.InvalidId;
        }

        DeleteResult dr = _languagesCollection.DeleteOne(lang => lang.Id == id);

        return dr.DeletedCount == 0 ? ResponseCode.NotFound : ResponseCode.Found;
    }
}