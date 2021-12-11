using System.Collections;
using MongoDB.Driver;
using Scandium.Entities;

namespace Scandium.Services;

public class GeneratorHelperService
{
    private readonly MongoDbContext _dbContext;
    
    public GeneratorHelperService(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Word>> GetVocabulary()
    {
        var result = await _dbContext.Words.FindAsync(FilterDefinition<Word>.Empty);
        return result.ToEnumerable();
    }
}