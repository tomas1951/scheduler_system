using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchedulerClientApp.Resources
{
    public class StatusMessage : BaseMessage
    {
        public string? CurrentStatus { get; set; }

        public StatusMessage(string currentStatus)
        {
            MessageType = "status";
            Date = DateTime.Now;
            CurrentStatus = currentStatus;
        }

        public string GetJsonString()
        {
            return JsonSerializer.Serialize(this);
        }

        public async Task<string> SaveToJsonFile()
        {
            string fileName = "baseMessage.json";
            using FileStream createStream = File.Create(fileName);
            await JsonSerializer.SerializeAsync(createStream, this);
            await createStream.DisposeAsync();
            return fileName;
        }
    }
}
