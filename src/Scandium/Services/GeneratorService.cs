using System.Collections;
using MongoDB.Driver;
using Scandium.Entities;

namespace Scandium.Services;

public class GeneratorService
{
    private readonly MongoDbContext _dbContext;
    
    public GeneratorService(MongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Word>> GetVocabulary()
    {
        var result = await _dbContext.Words.FindAsync(FilterDefinition<Word>.Empty);
        return result.ToEnumerable();
    }

    public async Task<string> DoJob(
        int numberOfWords,
        CancellationToken cancellationToken,
        IProgress<double> progressReporter)
    {
        
        var vocabulary = (await this.GetVocabulary()).ToList();
        var vocabularySize = vocabulary.Count;
        
        var random = new Random();

        var previousWord = ".";
        var result = string.Empty;

        for (var i = 0; i < numberOfWords; ++i)
        {
            if (cancellationToken.IsCancellationRequested) throw new TaskCanceledException();

            var wordNumber = random.Next(0, vocabularySize);
            var currentWord = vocabulary[wordNumber].Content;

            if (previousWord == currentWord)
            {
                --i;
                continue;
            }
            
            if (previousWord == ".")
            {
                var firstLetter = char.ToUpper(currentWord[0]);
                if (currentWord.Length == 1) currentWord = firstLetter.ToString();
                else currentWord = firstLetter + currentWord[1..];
            }

            previousWord = currentWord;

            result += currentWord;

            if (previousWord != "." && previousWord != "," && previousWord != " -")
            {
                var punctuationMark = string.Empty;
                
                if (random.NextDouble() <= 0.1) punctuationMark = ".";
                else if (random.NextDouble() <= 0.2) punctuationMark = ",";
                else if (random.NextDouble() <= 0.05) punctuationMark = " -";

                if (punctuationMark != string.Empty)
                {
                    result += punctuationMark;
                    previousWord = punctuationMark;
                }
            }

            result += ' ';
            
            progressReporter.Report(1 / (double)numberOfWords);

            await Task.Delay(50, cancellationToken);
        }

        return result;
    }
}