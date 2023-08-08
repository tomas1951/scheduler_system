using Newtonsoft.Json;

namespace Messages
{
    public class BaseMessage
    {
        public string? MessageType { get; set; }
        public string Date { get; set; }

        JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.All };

        public BaseMessage(string messageType)
        {
            MessageType = messageType;
            Date = GetDate();
        }

        public virtual string GetSerializedString()
        {
            //return JsonSerializer.Serialize(this);
            return JsonConvert.SerializeObject(this, JsonSettings);
        }

        private string GetDate()
        {
            DateTime currentDateTime = DateTime.Now;
            return currentDateTime.ToString("MM/dd/yyyy HH:mm:ss");
        }
    }
}
