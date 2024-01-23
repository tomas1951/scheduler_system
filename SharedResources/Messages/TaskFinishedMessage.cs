using Newtonsoft.Json;

namespace SharedResources.Messages;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
public class TaskFinishedMessage : BaseMessage
{
    public string TaskID { get; set; }

    private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public TaskFinishedMessage(string taskID) : base()
    {
        TaskID = taskID;
    }

    public override string GetSerializedString()
    {
        return JsonConvert.SerializeObject(this, JsonSettings);
    }
}
