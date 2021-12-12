using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Scandium.Entities;

namespace Scandium.Services;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    public readonly GridFSBucket GridFsBucket;

    public MongoDbContext(IMongoClient mongoClient, string databaseName)
    {
        _database = mongoClient.GetDatabase(databaseName);
        GridFsBucket = new GridFSBucket(_database);
    }
        
    // Collections
    public IMongoCollection<Word> Words => _database.GetCollection<Word>("words");
    public IMongoCollection<Job> Jobs => _database.GetCollection<Job>("jobs");
    public IMongoCollection<JobConnectionId> JobsConnectionId => 
        _database.GetCollection<JobConnectionId>("jobs-connection-id");
}