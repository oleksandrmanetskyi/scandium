using MongoDB.Bson;

namespace Scandium.Entities;

public class Word
{
    public ObjectId Id { get; init; }
    public string Content { get; set; } = string.Empty;
}