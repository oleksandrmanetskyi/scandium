using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace Scandium.Entities;

public class Job
{
    public ObjectId Id { get; init; } = new ();
    public DateTime CreateDateTime { get; init; } = DateTime.Now;
    public JobState State { get; set; }
    public string Result { get; set; } = string.Empty;
}

public enum JobState
{
    Running,
    Done,
    Canceled
}