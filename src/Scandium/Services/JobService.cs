using MongoDB.Driver;
using Scandium.Entities;

namespace Scandium.Services;

public class JobService
{
    private readonly MongoDbContext _dbContext;
    
    public JobService(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task CreateJob(string connectionId)
    {
        await _dbContext.Jobs.InsertOneAsync(new Job
        {
            ConnectionId = connectionId,
            State = JobState.Running
        });
    }

    public async Task<IEnumerable<Job>> GetJobs()
    {
        var result = await _dbContext.Jobs.FindAsync(FilterDefinition<Job>.Empty);
        return result.ToEnumerable();
    }

    public async Task DoneJob(string connectionId)
    {
        var filter = Builders<Job>.Filter.Eq(j => j.ConnectionId, connectionId);
        var update = Builders<Job>.Update.Set(j => j.State, JobState.Done);
        await _dbContext.Jobs.UpdateOneAsync(filter, update);
    }
    
    public async Task CancelJob(string connectionId)
    {
        var filter = Builders<Job>.Filter.Eq(j => j.ConnectionId, connectionId);
        var update = Builders<Job>.Update.Set(j => j.State, JobState.Canceled);
        await _dbContext.Jobs.UpdateOneAsync(filter, update);
    }
}