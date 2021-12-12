using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Scandium.Entities;
using Scandium.Models;
using Scandium.Services;
using Scandium.SignalR;

namespace Scandium.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly JobProgressReporterFactory _jobProgressReporterFactory;
    private readonly GeneratorService _generatorService;
    private readonly JobService _jobService;

    public HomeController(
        ILogger<HomeController> logger, 
        JobProgressReporterFactory jobProgressReporterFactory, 
        GeneratorService generatorService,
        JobService jobService
        )
    {
        _logger = logger;
        _jobProgressReporterFactory = jobProgressReporterFactory;
        _generatorService = generatorService;
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
        if (await _jobService.GetActiveJobsCount() >= 2)
            return new BadRequestObjectResult("Too much job running at the same time");
        
        _logger.LogInformation(
            "{DateTime.Now} - {Dns.GetHostName()} - Start generating {model.NumberOfWords} words", 
            DateTime.Now, Dns.GetHostName(), model.NumberOfWords);
        
        var job = await _jobService.CreateJob();
        var progressReporter = _jobProgressReporterFactory.GetLoadingBarReporter(job.Id);
        var stateReporter = _jobProgressReporterFactory.GetStateReporter(job.Id);
        
        #pragma warning disable CS4014
        Task.Run( async () =>
        {
            try
            {
                await _generatorService.DoJob(
                        model.NumberOfWords,
                        cancellationToken,
                        progressReporter)
                    .ContinueWith(result => _jobService.DoneJob(job.Id, result.Result),
                        cancellationToken);
                stateReporter.ReportDoneState();
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception: {e}. Job id: {job.Id}");
                await _jobService.CancelJob(job.Id);
                stateReporter.ReportCancelState();
            }
        }, cancellationToken);
        #pragma warning restore CS4014

        return new OkObjectResult("Job started");
    }

    [HttpGet("{id}")]
    [Route("job-result/{id}")]
    public async Task<IActionResult> GetJobResult()
    {
        return NoContent();
    }

    [HttpGet]
    [Route("job-list")]
    public async Task<IActionResult> JobList()
    {
        var content = await _jobService.GetJobs();
        return PartialView("_JobListPartial", content);

    }

    [HttpGet("{id}")]
    [Route("job-details/{id}")]
    public async Task<IActionResult> JobDetails(string id)
    {
        var job = await _jobService.GetJob(new ObjectId(id));
        return View(new JobDetailsViewModel
        {
            Id = job.Id.ToString(),
            CreateDateTime = job.CreateDateTime,
            State = job.State,
            Result = job.Result
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}