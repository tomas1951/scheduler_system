using Newtonsoft.Json;
using SchedulerClientApp.Services;
using SchedulerClientApp.ViewModels;
using SharedResources.Enums;
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
    public LogService LogService { get; set; }
    //public ClientStatusEnum Status = ClientStatusEnum.Disconnected;

    // Log macro
    public delegate void MacroDelegate(string message, bool endl = true,
        bool date = true);
    private readonly MacroDelegate Log;

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
        Log = LogService.Log;
    }

    //private void ConnectToServer()
    //{
    //    Log("Connecting to a server...");
    //    try
    //    {
    //        Client.Connect("127.0.0.1", 1234);
    //    }
    //    catch (SocketException)
    //    {
    //        Log("Server is offline");
    //    }
    //    catch (Exception ex)
    //    {
    //        Log($"Exception: {ex.GetType().Name} - {ex.Message}");
    //    }

    //    if (Client is not null && Client.IsConnected())
    //    {
    //        Log("Connection successful");
    //        ClientStatus = ClientStatus.Connected;
    //        OnStatusTimer(null);
    //    }
    //    else
    //    {
    //        Log("Connection failed");
    //        ClientStatus = ClientStatus.Disconnected;
    //    }
    //}

    public ClientStatus Connect(string address, int port)
    {
        
        bool exceptionCaught = false;

        try
        {
            TcpClient.Connect(address, port);
        }
        catch (SocketException)
        {
            Log($"Server is offline.");
            exceptionCaught = true;
        }
        catch (Exception ex)
        {
            Log($"Exception: {ex.GetType().Name} - {ex.Message}");
            exceptionCaught = true;
        }

        if (!exceptionCaught && IsConnected())
        {
            TcpClient.ReceiveBufferSize = 1024;
            TcpClient.SendBufferSize = 1024;

            Thread receivingThread = new Thread(ReceivingMethod);
            receivingThread.IsBackground = true;
            receivingThread.Start();

            Thread sendingThread = new Thread(SendingMethod);
            sendingThread.IsBackground = true;
            sendingThread.Start();

            Log("Connection successful");
            return ClientStatus.Connected;
        }

        return ClientStatus.Disconnected;
    }

    public bool IsConnected()
    {
        Socket sock = TcpClient.Client;
        bool blockingState = sock.Blocking;
        try
        {
            byte [] tmp = new byte[1];

            sock.Blocking = false;
            sock.Send(tmp, 0, 0);
            return true;
        }
        catch (SocketException e) 
        {
            // 10035 == WSAEWOULDBLOCK
            if (e.NativeErrorCode.Equals(10035))
                return true;
            else
            {
                return false;
            }
        }
        finally
        {
            sock.Blocking = blockingState;
        }
    }

    public ClientStatus GetConnectionStatus()
    {
        return IsConnected() ? ClientStatus.Connected : ClientStatus.Disconnected;
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
                    BaseMessage? json_msg = JsonConvert
                        .DeserializeObject<BaseMessage>(str ?? "", JsonSettings);

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
                        // NOTE - repair that client in this
                        PrintMessage(TcpClient, json_msg); 
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
