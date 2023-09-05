﻿using Newtonsoft.Json;
using SchedulerClientApp.Services;
using SharedResources.Enums;
using SharedResources.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace SchedulerClientApp.ClientModule;

/// <summary>
/// Scheduler client class of a scheduler system.
/// </summary>
public class SchedulerClient : ISchedulerClient
{
    // Class properties
    public LogService LogService { get; set; }
    public TcpClient TcpClient { get; set; }
    public List<BaseMessage> MessageQueue { get; set; }

    // Macro for simpler way of logging messages
    public delegate void MacroDelegate(string message, bool endl = true,
        bool date = true);
    private readonly MacroDelegate Log;

    // Settings for serializing/deserialising json messages
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

    // Creates a socket to the server side using function parameters.
    public ClientStatus Connect(string address, int port)
    {
        TcpClient = new TcpClient();
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

    // Checks whether client is connected to the server.
    public bool IsConnected()
    {
        if (!TcpClient.Connected)
        {
            return false;
        }

        try
        {
            TcpClient.Client.Send(new byte[1], 0, 0);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    // Checks and returns connection status.
    public ClientStatus GetConnectionStatus()
    {
        return IsConnected() ? ClientStatus.Connected : ClientStatus.Disconnected;
    }

    // Closes the connection with the server.
    public void Disconnect()
    {
        TcpClient.GetStream().Close();
        TcpClient.Close();
    }

    // Adds a message to the MessageQueue to be sent.
    public void SendMessage(BaseMessage message)
    {
        MessageQueue.Add(message);
    }

    // Sends a message to the server if there is some message waiting in MessageQueue.
    // This method runs in its own thread.
    public void SendingMethod()
    {
        while (IsConnected())
        {
            if (MessageQueue.Count > 0)
            {
                NetworkStream stream = TcpClient.GetStream();
                StreamWriter writer = new StreamWriter(stream);

                BaseMessage message = MessageQueue[0];
                string str = message.GetSerializedString();

                try
                {
                    writer.WriteLine(str);
                    writer.Flush();
                    MessageQueue.Remove(message);
                }
                catch (Exception)
                {
                    Disconnect();
                }
            }
            Thread.Sleep(1000);
        }
    }

    // Listens for a incomming messages.
    // This method runs in its own thread.
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
                    Disconnect();
                }
            }
            Thread.Sleep(1000);
        }
    }

    // Prints incomming Status message into log.
    private void PrintMessage(TcpClient client, StatusMessage message)
    {
        Log($"Host: {GetClientIP(client)}, Type: Status, Content: " +
            $"{message.CurrentStatus}");
    }

    // Prints incomming Base message into log.
    private void PrintMessage(TcpClient client, BaseMessage message)
    {
        Log($"Host: {GetClientIP(client)}, Type: Base, Content: Empty");
    }

    // Returns client ip address for given client class argument.
    // If client is null, function returns empty string.
    private static string GetClientIP(TcpClient client)
    {
        string? name = client.Client.RemoteEndPoint?.ToString();
        return (name is not null) ? name : "";
    }
}
