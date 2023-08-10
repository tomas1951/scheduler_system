﻿using System.Net.Sockets;
using System.Text;
using System.Timers;
using Newtonsoft.Json;
using SharedLibMessages;

namespace SchedulerServerSideApp;

public class Server
{
    private int Port;
    private bool IsStarted = false;
    private TcpListener? Listener;
    private List<TcpClient> ConnectedClients = new List<TcpClient>();

    // Thread signal
    public static ManualResetEvent tcpClientConnected = 
        new ManualResetEvent(false);
    
    // Listening timer
    private System.Timers.Timer? ListeningTimer;
    
    // Handlers
    public delegate void MessageReceivedHandler(object? sender, MessageFromClient m);
    private static event MessageReceivedHandler? MessageReceivedEvent;
    
    // For serializing json messages
    private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    public Server(int port)
    {
        Port = port;
        MessageReceivedEvent += OnMessageReceived;
    }

    public void Start()
    {
        if (IsStarted)
        {
            return;
        }

        Listener = new TcpListener(System.Net.IPAddress.Any, Port);
        Listener.Start();
        IsStarted = true;

        SetListeningTimer();
        ListenForNewClients();
    }

    private void SetListeningTimer()
    {
        ListeningTimer = new System.Timers.Timer(5000);
        ListeningTimer.Elapsed += new ElapsedEventHandler(OnListeningTimer);
        ListeningTimer.AutoReset = true;
        ListeningTimer.Enabled = true;
    }

    private void OnListeningTimer(object? source, ElapsedEventArgs e)
    {
        ListenForNewClients();
    }

    private  async void ListenForNewClients()
    {
        if (Listener is not null)
        {
            //Console.WriteLine("Listening for new clients.");
            TcpClient client = await Listener.AcceptTcpClientAsync();  
            OnClientConnected(client);
        }
    }

    public void Stop()
    {
        if (IsStarted)
        {
            Listener?.Stop();
            IsStarted = false;
        }
    }

    private void OnClientConnected(TcpClient client)
    {
        Console.WriteLine("Client connected: {0}", 
            client.Client.RemoteEndPoint?.ToString());
        Thread clientThread = new Thread(
            () => HandleClient(client) 
        );
        clientThread.Start();
        ConnectedClients.Add(client);
    }

    private void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                // Raise the OnMessageReceived event
                MessageReceivedEvent?.Invoke(null, 
                    new MessageFromClient(client, message));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            Console.WriteLine("Client {0} left the server. (Unable to read data)",
                GetClientIP(client));
            ConnectedClients.Remove(client);
            client.Close();
            ListenForNewClients();
        }
    }

    private static void OnMessageReceived(object? sender, MessageFromClient message)
    {
        //Console.WriteLine($"Client {GetClientIP(message.Client)} sends " +
        //    $"raw message: {message.Content}");

        BaseMessage? json_msg = JsonConvert.DeserializeObject<BaseMessage>(
            message.Content, JsonSettings);
        if (json_msg is null)
        { 
            Console.WriteLine("Message was empty");
            return;
        }
        if (json_msg is StatusMessage)
        {
            PrintMessage(message.Client, (StatusMessage)json_msg);
        }
        else
        {
            PrintMessage(message.Client, json_msg);
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

    private async void SendMessageToClient(TcpClient client, BaseMessage message)
    {
        await Task.Run( () =>
        {
            if (!client.Connected)
            {
                Console.WriteLine("Client offline.");
                return;
            }

            try
            {
                NetworkStream stream = client.GetStream();
                StreamWriter writer = new StreamWriter(stream);

                string str = message.GetSerializedString();

                writer.WriteLine(str);
                writer.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message} {ex.GetType()}");
                client.GetStream().Close();
                client.Close();
            }
        });
    }

    public void SendTask()
    {
        TcpClient client = ConnectedClients[0]; 

        TaskMessage newTask = new TaskMessage();
        newTask.ExeFilePath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Audacity.lnk";

        SendMessageToClient(client, newTask);
    }
}
