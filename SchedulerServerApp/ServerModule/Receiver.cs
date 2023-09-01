using Newtonsoft.Json;
using SharedResources.Enums;
using SharedResources.Messages;
using System.Net.Sockets;

namespace SchedulerServerApp.ServerModule;

internal class Receiver
{
    private TcpClient Client;
    private Server Server;
    private List<BaseMessage> MessageQueue = new List<BaseMessage>();
    private ClientStatus Status = ClientStatus.Disconnected;
    JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    public Receiver(TcpClient client, Server server)
    {
        Client = client;
        Server = server; // may not be necessary (its for reaching other clients)
        Client.ReceiveBufferSize = 1024;
        Client.SendBufferSize = 1024;
    }

    public void Start()
    {
        Console.WriteLine("creating receiving and sending thread");

        Thread receivingThread = new Thread(ReceivingMethod);
        receivingThread.IsBackground = true;
        receivingThread.Start();

        Thread sendingThread = new Thread(SendingMethod);
        sendingThread.IsBackground = true;
        sendingThread.Start();
    }

    public void SendMessage(BaseMessage message)
    {
        MessageQueue.Add(message);
    }

    public void Disconnect()
    {
        Client.GetStream().Close();
        Client.Close();
    }

    private void SendingMethod()
    {
        while (Status != ClientStatus.Disconnected)
        {
            if (MessageQueue.Count > 0)
            {
                NetworkStream stream = Client.GetStream();
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
                }
                MessageQueue.Remove(message);
            }
            Thread.Sleep(30);
        }
    }

    private void ReceivingMethod()
    {
        while (Status != ClientStatus.Disconnected)
        {
            if (Client.Available > 0)
            {
                NetworkStream nwStream = Client.GetStream();
                StreamReader reader = new StreamReader(Client.GetStream());

                try
                {
                    string? inputString = reader.ReadLine();
                    if (inputString is null)
                    {
                        Console.WriteLine("inputString was none -> skip");
                        Console.WriteLine("Client {0} left the server. (Unable to " +
                            "read data)", GetClientIP(Client));
                        break;
                    }

                    var msg = JsonConvert.DeserializeObject<BaseMessage>(inputString,
                        JsonSettings);
                    if (msg is null)
                    {
                        Console.WriteLine("msg was none -> skip");
                        break;
                    }

                    if (msg is StatusMessage)
                    {
                        PrintMessage(Client, (StatusMessage)msg);
                    }
                    else
                    {
                        Type type = msg.GetType();
                        Console.WriteLine("Message type: {0}", type.ToString());
                    }

                    OnMessageReceived(msg);
                }
                catch (IOException ex)
                {
                    if (ex.Message.StartsWith("Unable to read data"))
                    {
                        Console.WriteLine("Client {0} left the server", 
                            Client.Client.RemoteEndPoint?.ToString());
                    }
                    else
                    {
                        Console.WriteLine("Exception: {0} {1}", ex.Message, 
                            ex.GetType().Name);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: {0} {1}", ex.Message, 
                        ex.GetType().Name);
                    break;
                }
            }
            Thread.Sleep(30);
        }
        Client.Close();
        Status = ClientStatus.Disconnected;
    }

    private void OnMessageReceived(BaseMessage msg)
    {
        Console.WriteLine("On message Received method called - I can do anything " +
            "from here");
    }

    private string GetClientIP(TcpClient client)
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

    private void PrintMessage(TcpClient client, StatusMessage message)
    {
        Console.WriteLine(string.Format("Host: {0}, Status: {1}", GetClientIP(client),
            message.CurrentStatus));
    }
}
