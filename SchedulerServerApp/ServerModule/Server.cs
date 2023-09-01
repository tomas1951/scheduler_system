using System.Net.Sockets;
using System.Text;
using System.Timers;
using Newtonsoft.Json;
using SharedResources.Messages;

namespace SchedulerServerApp.ServerModule;

public class Server : IServer
{
    public int Port { get; set; }
    public bool IsStarted { get; set; }
    public TcpListener Listener { get; set; }
    public List<TcpClient> ConnectedClients { get; set; }

    // Thread signal
    public static ManualResetEvent tcpClientConnected =
        new ManualResetEvent(false);

    // Listening timer
    public System.Timers.Timer ListeningTimer { get; set; }

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
        ConnectedClients = new List<TcpClient>();
        MessageReceivedEvent += OnMessageReceived;

        Listener = new TcpListener(System.Net.IPAddress.Any, Port);
        Listener.Start();
        IsStarted = true;

        ListeningTimer = new System.Timers.Timer(5000);
        SetListeningTimer();
        ListenForNewClients();

        Console.WriteLine("Server is online.");
    }

    public void IsOnline()
    {

    }

    private void SetListeningTimer()
    {
        ListeningTimer.Elapsed += new ElapsedEventHandler(OnListeningTimer);
        ListeningTimer.AutoReset = true;
        ListeningTimer.Enabled = true;
    }

    public void OnListeningTimer(object? source, ElapsedEventArgs e)
    {
        ListenForNewClients();
    }

    public async void ListenForNewClients()
    {
        if (Listener is not null)
        {
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

    public void OnClientConnected(TcpClient client)
    {
        Console.WriteLine($"Client connected: {GetClientIP(client)}");
        Thread clientThread = new Thread(
            () => HandleClient(client)
        );
        clientThread.Start();
        ConnectedClients.Add(client);
    }

    public void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                if (client is null)
                {
                    throw new NullReferenceException("Client reference is null.");
                }
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                // Raise the OnMessageReceived event
                MessageReceivedEvent?.Invoke(null,
                    new MessageFromClient(client, message));
            }
        }
        catch (Exception e)
        {
            if (client is null)
            {
                Console.WriteLine("Client disconnected.");
            }
            else
            {
                Console.Write($"Client {GetClientIP(client)} left the server. " +
                    $"(Unable to read data) ");
                Console.WriteLine($"Exception while reading client " +
                    $"{GetClientIP(client)} message: {e.Message}");
                DisconnectClient(client);
                ListenForNewClients();
            }
        }
    }

    public void OnMessageReceived(object? sender, MessageFromClient message)
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
        string? name = client?.Client?.RemoteEndPoint?.ToString() ?? null;
        if (name is not null)
        {
            return name;
        }
        else
        {
            return "--name unknown--";
        }
    }

    public async void SendMessageToClient(TcpClient client, BaseMessage message)
    {
        await Task.Run(() =>
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

    public void TestSendTask()
    {
        TcpClient? client = ConnectedClients.LastOrDefault();

        if (client is null)
        {
            Console.WriteLine("No connected clients.");
            return;
        }

        TaskMessage newTask = new TaskMessage();
        newTask.ExeFilePath = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs
                                \Audacity.lnk";

        SendMessageToClient(client, newTask);
    }

    public void TestDisconnect()
    {
        TcpClient? client = ConnectedClients.LastOrDefault();
        
        if (client is null)
        {
            Console.WriteLine("No connected clients.");
            return;
        }

        DisconnectClient(client);
    }

    public void DisconnectClient(TcpClient client)
    {
        ConnectedClients.Remove(client);
        client?.Close();
    }
}
