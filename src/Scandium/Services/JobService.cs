using MongoDB.Bson;
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
    
    public async Task<Job> CreateJob()
    {
        var job = new Job
        {
            Id = new ObjectId(),
            State = JobState.Running
        };
        await _dbContext.Jobs.InsertOneAsync(job);
        return job;
    }

    public async Task<IEnumerable<Job>> GetJobs()
    {
        var result = await _dbContext.Jobs.FindAsync(FilterDefinition<Job>.Empty);
        return result.ToEnumerable();
    }
    
    public async Task<long> GetActiveJobsCount()
    {
        var filter = Builders<Job>.Filter.Eq(j => j.State, JobState.Running);
        var result = await _dbContext.Jobs.CountDocumentsAsync(filter);
        return result;
    }
    
    public async Task<Job> GetJob(ObjectId id)
    {
        var result = await _dbContext.Jobs.FindAsync(j => j.Id == id);
        return result.Single();
    }

    public async Task DoneJob(ObjectId id, string result)
    {
        var filter = Builders<Job>.Filter.Eq(j => j.Id, id);
        var update = Builders<Job>.Update
            .Set(j => j.State, JobState.Done)
            .Set(j => j.Result, result);
        await _dbContext.Jobs.UpdateOneAsync(filter, update);
    }
    
    public async Task CancelJob(ObjectId id)
    {
        var filter = Builders<Job>.Filter.Eq(j => j.Id, id);
        var update = Builders<Job>.Update.Set(j => j.State, JobState.Canceled);
        await _dbContext.Jobs.UpdateOneAsync(filter, update);
    }

    public async Task<List<string>> GetJobConnectionIdList(ObjectId jobId)
    {
        var filter = Builders<JobConnectionId>.Filter.Eq(j => j.JobId, jobId);
        var allConnectionId = (await _dbContext.JobsConnectionId.FindAsync(filter)).ToList();
        var result = new List<string>();
        if (allConnectionId.Any())
        {
            result.AddRange(allConnectionId.Select(c => c.ConnectionId));
        }
        return result;
    }

    public async Task SubscribeConnectionId(ObjectId jobId, string connectionId)
    {
        await _dbContext.JobsConnectionId.InsertOneAsync(new JobConnectionId
        {
            ConnectionId = connectionId,
            JobId = jobId
        });
    }
}