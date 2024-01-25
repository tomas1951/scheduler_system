using Newtonsoft.Json;

namespace SharedResources.Messages;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
public class TaskMessage : BaseMessage
{
    public int ID { get; set; }  // TODO - why not integer? 
    public string Name { get; set; }
    public string Description { get; set; }
    public int Group { get; set; }
    public Enums.SchedulerTaskStatus Status { get; set; }
    public int Priority { get; set; }
    public DateTime TimeCreated { get; set; }
    public string ExeFilePath { get; set; }
    public string InputFilesPath { get; set; }
    public string OutputFilesPath { get; set; }
    public string OperatingSystem { get; set; }
    public DateTime TimeCompleted { get; set; }
    public int UserID { get; set; }
    // TODO - possible missynchronization of this class due to change of properties

    private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public TaskMessage(
        int iD,
        string name,
        string description,
        int group, 
        Enums.SchedulerTaskStatus status,
        int priority, 
        DateTime timeCreated, 
        string exeFilePath, 
        string inputFilesPath, 
        string outputFilesPath, 
        string operatingSystem, 
        DateTime timeCompleted,
        int userID)
    {
        ID = iD;
        Name = name;
        Description = description;
        Group = group;
        Status = status;
        Priority = priority;
        TimeCreated = timeCreated;
        ExeFilePath = exeFilePath;
        InputFilesPath = inputFilesPath;
        OutputFilesPath = outputFilesPath;
        OperatingSystem = operatingSystem;
        TimeCompleted = timeCompleted;
        UserID = userID;
    }

    public TaskMessage()
    {
        ID = 999;
        Name = "DefaultName";
        Description = "Default description";
        Group = 999;
        Status = Enums.SchedulerTaskStatus.Waiting;
        Priority = 5;
        TimeCreated = DateTime.Now;
        ExeFilePath = "path.exe";
        InputFilesPath = "/input_path/";
        OutputFilesPath = "/output_path/";
        OperatingSystem = "DefaultOS";
        UserID = 999;
    }

    public override string GetSerializedString()
    {
        return JsonConvert.SerializeObject(this, JsonSettings);
    }
}
