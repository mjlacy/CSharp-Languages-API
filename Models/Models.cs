using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CSharp_Languages_API.Models;

public static class Constants {
    public const string AppName = "CSharp-Languages API";
    public const string Version = "0.0.1";
    public const int PingTimeoutMs = 1000;
}

public enum ResponseCode : long {
    Found = 0,
    InvalidId = -1,
    NotFound = 1,
}

public class LanguagesDatabaseSettings {
    public required string ConnectionString { get; set; }

    public required string DatabaseName { get; set; }

    public required string LanguagesCollectionName { get; set; }
}

public class Languages {
    [JsonPropertyName("languages")]
    public List<Language> languages { get; set; }

    public Languages(List<Language> languages)
    {
        this.languages = languages;
    }
}

public class Language {
    [BsonId]
    [BsonElement("_id")]
    [JsonPropertyName("_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [BsonElement("creators")]
    [JsonPropertyName("creators")]
    public string[]? Creators { get; set; }

    [BsonElement("extensions")]
    [JsonPropertyName("extensions")]
    public string[]? Extensions { get; set; }

    [BsonElement("firstAppeared")]
    [JsonPropertyName("firstAppeared")]
    public DateTime? FirstAppeared { get; set; }

    [BsonElement("year")]
    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [BsonElement("wiki")]
    [JsonPropertyName("wiki")]
    public string? Wiki { get; set; }
}

public class Info {
    [JsonPropertyName("ApplicationName")]
    public required string ApplicationName { get; set; }

    [JsonPropertyName("Version")]
    public required string Version { get; set; }
}

public class HealthCodes {
    [JsonPropertyName("Application")]
    public required string Application { get; set; }

    [JsonPropertyName("MongoConnection")]
    public required string MongoConnection { get; set; }
}

public class HealthCheck {
    [JsonPropertyName("Info")]
    public required Info Info { get; set; }

    [JsonPropertyName("HealthCodes")]
    public required HealthCodes HealthCodes { get; set; }
}