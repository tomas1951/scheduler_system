﻿using Newtonsoft.Json;

namespace SharedResources.Messages;

public class BaseMessage
{
    public string Date { get; set; }

    JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

    public BaseMessage()
    {
        Date = GetDate();
    }

    public virtual string GetSerializedString()
    {
        return JsonConvert.SerializeObject(this, JsonSettings);
    }

    private string GetDate()
    {
        DateTime currentDateTime = DateTime.Now;
        return currentDateTime.ToString("MM/dd/yyyy HH:mm:ss");
    }
}
