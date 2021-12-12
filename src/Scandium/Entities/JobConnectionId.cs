using MongoDB.Bson;

namespace Scandium.Entities;

public class JobConnectionId
{
    public ObjectId Id { get; init; }
    public ObjectId JobId { get; init; }
    public string ConnectionId { get; set; } = string.Empty;
}