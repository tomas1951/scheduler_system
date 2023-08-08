using Newtonsoft.Json;

namespace Messages
{
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
    public class StatusMessage : BaseMessage
    {
        public string CurrentStatus { get; set; }
        JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public StatusMessage(string currentStatus) : base("status")
        {
            CurrentStatus = currentStatus;
        }

        public override string GetSerializedString()
        {
            return JsonConvert.SerializeObject(this, JsonSettings);
        }
    }
}
