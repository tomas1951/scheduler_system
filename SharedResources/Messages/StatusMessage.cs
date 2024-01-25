using Newtonsoft.Json;

namespace SharedResources.Messages;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
public class StatusMessage : BaseMessage
{
    public string CurrentStatus { get; set; }
    public bool CurrentTask { get; set; }

    private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

    //public StatusMessage(string currentStatus) : base()
    //{
    //    CurrentStatus = currentStatus;
    //    CurrentTask = false;
    //}

    public StatusMessage(string currentStatus, bool currentTask) : base()
    {
        CurrentStatus = currentStatus;
        CurrentTask = currentTask;
    }

    public override string GetSerializedString()
    {
        return JsonConvert.SerializeObject(this, JsonSettings);
    }
}
