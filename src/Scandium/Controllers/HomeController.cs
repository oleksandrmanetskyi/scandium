using System.Diagnostics;
using System.Net;
using System.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Scandium.Models;
using Scandium.Services;
using Scandium.SignalR;

namespace Scandium.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ProgressReporterFactory _progressReporterFactory;
    private readonly GeneratorHelperService _generatorHelperService;
    private readonly JobService _jobService;

    public HomeController(
        ILogger<HomeController> logger, 
        ProgressReporterFactory progressReporterFactory, 
        GeneratorHelperService generatorHelperService,
        JobService jobService
        )
    {
        _logger = logger;
        _progressReporterFactory = progressReporterFactory;
        _generatorHelperService = generatorHelperService;
        _jobService = jobService;
    }

    public IActionResult Index()
    {
        ViewData["HostName"] = Dns.GetHostName();
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    [Route("generate")]
    public async Task<IActionResult> Generate(GeneratorInputViewModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Start generating {model.NumberOfWords} words. ConnectionId: {model.ConnectionId}");
        
        await _jobService.CreateJob(model.ConnectionId);
        
        var progressReporter = _progressReporterFactory.GetLoadingBarReporter(model.ConnectionId);
        var vocabulary = (await _generatorHelperService.GetVocabulary()).ToList();
        var vocabularySize = vocabulary.Count();
        
        var random = new Random();

        var previousWord = ".";
        var result = string.Empty;

        for (var i = 0; i < model.NumberOfWords; ++i)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await _jobService.CancelJob(model.ConnectionId);
                return Content("Canceled");
            }
            
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
                else currentWord = firstLetter + currentWord.Substring(1);
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
            
            progressReporter.Report(1 / (double)model.NumberOfWords);

            await Task.Delay(50);
        }
        
        await _jobService.DoneJob(model.ConnectionId);

        return Content(result);
    }

    [HttpGet]
    [Route("job-list")]
    public async Task<IActionResult> JobList()
    {
        var content = await _jobService.GetJobs();
        return PartialView("_JobListPartial", content);

    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}