using Newtonsoft.Json;

namespace SharedResources.Messages;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
public class TaskMessage : BaseMessage
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string UserID { get; set; }
    public string BucketId { get; set; }
    public Enums.SchedulerTaskStatus Status { get; set; }
    public int Priority { get; set; }
    public DateTime TimeCreated { get; set; }
    public string ExeFilePath { get; set; }
    public string InputFilesPath { get; set; }
    public string OutputFilesPath { get; set; }
    public string OperatingSystem { get; set; }

    private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public TaskMessage(string iD, string name, string userID, string bucketId, 
                       Enums.SchedulerTaskStatus status, int priority, DateTime timeCreated, 
                       string exeFilePath, string inputFilesPath, string outputFilesPath, 
                       string operatingSystem)
    {
        ID = iD;
        Name = name;
        UserID = userID;
        BucketId = bucketId;
        Status = status;
        Priority = priority;
        TimeCreated = timeCreated;
        ExeFilePath = exeFilePath;
        InputFilesPath = inputFilesPath;
        OutputFilesPath = outputFilesPath;
        OperatingSystem = operatingSystem;
    }

    public TaskMessage()
    {
        ID = "DefaultID";
        Name = "DefaultName";
        UserID = "DefaultUserID";
        BucketId = "DefaultBucketID";
        Status = Enums.SchedulerTaskStatus.Waiting;
        Priority = 0;
        TimeCreated = DateTime.Now;
        ExeFilePath = "";
        InputFilesPath = "";
        OutputFilesPath = "";
        OperatingSystem = "DefaultOS";
    }

    public override string GetSerializedString()
    {
        return JsonConvert.SerializeObject(this, JsonSettings);
    }
}
