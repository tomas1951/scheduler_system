using Newtonsoft.Json;

namespace SharedResources.Messages;

[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
public class ConfirmationMessage : BaseMessage
{
    public string TaskID { get; set; }
    private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public ConfirmationMessage(string taskID) : base()
    {
        TaskID = taskID;
    }

    public override string GetSerializedString()
    {
        return JsonConvert.SerializeObject(this, JsonSettings);
    }
}
