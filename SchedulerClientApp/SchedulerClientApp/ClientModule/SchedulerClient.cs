using Newtonsoft.Json;
using SchedulerClientApp.Services;
using SharedResources.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace SchedulerClientApp.ClientModule;

public class SchedulerClient : ISchedulerClient
{
    public TcpClient TcpClient { get; set; }
    public List<BaseMessage> MessageQueue { get; set; }
    //public ClientStatusEnum Status = ClientStatusEnum.Disconnected;
    public LogService LogService { get; set; }
    
    // For serializing json messages
    private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    public SchedulerClient(LogService logService)
    {
        LogService = logService;
        TcpClient = new TcpClient();
        MessageQueue = new List<BaseMessage>();
    }

    public void Connect(string address, int port)
    {
        TcpClient = new TcpClient();
        TcpClient.Connect(address, port);
        //Status = ClientStatusEnum.Connected;
        TcpClient.ReceiveBufferSize = 1024;
        TcpClient.SendBufferSize = 1024;

        Thread receivingThread = new Thread(ReceivingMethod);
        receivingThread.IsBackground = true;
        receivingThread.Start();

        Thread sendingThread = new Thread(SendingMethod);
        sendingThread.IsBackground = true;
        sendingThread.Start();
    }

    public bool IsConnected()
    {
        return TcpClient.Connected; // NOTE - does it really test connectivity?
    }

    public void Disconnect()
    {
        //Status = ClientStatusEnum.Disconnected;
        TcpClient.GetStream().Close();
        TcpClient.Close();
    }

    public void SendMessage(BaseMessage message)
    {
        MessageQueue.Add(message);
    }

    public void SendingMethod()
    {
        while (IsConnected())
        {
            if (MessageQueue.Count > 0 && TcpClient is not null)
            {
                NetworkStream stream = TcpClient.GetStream();
                StreamWriter writer = new StreamWriter(stream);

                BaseMessage message = MessageQueue[0];
                string str = message.GetSerializedString();

                try
                {
                    writer.WriteLine(str);
                    writer.Flush();
                }
                catch
                {
                    Disconnect();
                    //Status = ClientStatusEnum.Disconnected;
                }
                MessageQueue.Remove(message);
            }
            Thread.Sleep(30);
        }
    }
 
    public void ReceivingMethod()
    {
        while (IsConnected())
        {
            if (TcpClient.Available > 0)
            {
                try
                {
                    NetworkStream stream = TcpClient.GetStream();
                    StreamReader reader = new StreamReader(stream);

                    string? str = reader.ReadLine();
                    BaseMessage? json_msg = JsonConvert.DeserializeObject<BaseMessage>(str ?? "", JsonSettings);

                    if (json_msg is null || str is null)
                    {
                        Console.WriteLine("Message was empty");
                        continue;
                    }
                    else if (json_msg is StatusMessage)
                    {
                        PrintMessage(TcpClient, (StatusMessage)json_msg); // DETTO
                    }
                    else
                    {
                        PrintMessage(TcpClient, json_msg); // NOTE - repair that client in this 
                        // scenary is a server
                    }
                }
                catch (Exception ex)
                {
                    LogService.Log(ex.Message);
                    TcpClient.Close();
                    //Status = ClientStatusEnum.Disconnected;
                }
            }
        }
    }

    private static void PrintMessage(TcpClient client, StatusMessage message)
    {
        Console.WriteLine(string.Format("Host: {0}, Type: Status, Content: {1}", 
            GetClientIP(client), message.CurrentStatus));
    }

    private static void PrintMessage(TcpClient client, BaseMessage message)
    {
        Console.WriteLine($"Host: {GetClientIP(client)}, Type: Base, Content: Empty");
    }

    private static string GetClientIP(TcpClient client)
    {
        string? name = client.Client.RemoteEndPoint?.ToString();
        if (name is not null)
        {
            return name;
        }
        else
        {
            return "";
        }
    }

}
