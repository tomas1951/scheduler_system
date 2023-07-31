using SchedulerClientApp.ClientModule;
using SchedulerClientApp.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;

namespace SchedulerClientApp.Modules
{
    public class Client
    {
        //public string? Address;
        //public int? Port;
        public TcpClient? TcpClient;
        //public NetworkStream? ClientSockStream;
        //public StreamReader? ClientStreamReader;
        //public StreamWriter? ClientStreamWriter;
        public List<BaseMessage> MessageQueue = new List<BaseMessage>();
        public ClientStatus Status = ClientStatus.Disconnected;

        public Client()
        {
            //Address = address;
            //Port = port;

            //TcpConnection = new TcpClient(Address, Port);
            //ClientSockStream = TcpConnection.GetStream();
            //ClientStreamReader = new StreamReader(ClientSockStream);
            //ClientStreamWriter = new StreamWriter(ClientSockStream);
        }

        public void Connect(string address, int port)
        {
            //Address = address;
            //Port = port;

            TcpClient = new TcpClient();
            TcpClient.Connect(address, port);
            Status = ClientStatus.Connected;
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
            return TcpClient.Connected;
        }

        public void Disconnect()
        {
            TcpClient.GetStream().Close();
            TcpClient.Close();
        }

        public void SendMessage(BaseMessage message)
        {
            Console.WriteLine("TESTING CONSOLE - send message");
            MessageQueue.Add(message);
        }

        public void SendingMethod()
        {
            while (Status != ClientStatus.Disconnected)
            {
                if (MessageQueue.Count > 0 && TcpClient is not null)
                {
                    var m = MessageQueue[0];
                    Type type = m.GetType();
                    string s;

                    if (type == typeof(StatusMessage))
                    {
                        StatusMessage message = (StatusMessage)m;
                        s = JsonSerializer.Serialize(message);
                    }
                    else
                    {
                        BaseMessage message = (BaseMessage)m;
                        s = JsonSerializer.Serialize(message);
                    }

                    NetworkStream stream = TcpClient.GetStream();
                    StreamWriter writer = new StreamWriter(stream);
                    try
                    {
                        //f.Serialize(TcpClient.GetStream(), m);
                        writer.WriteLine(s);
                        writer.Flush();
                    }
                    catch
                    {
                        Disconnect();
                    }
                    MessageQueue.Remove(m);
                }
                Thread.Sleep(30);
            }
        }

        public void ReceivingMethod()
        {

        }


        // old
        //public bool SendMessage(byte[] message)
        //{
        //    if (IsConnected())
        //    {
        //        //ClientStreamWriter.WriteLine(message);
        //        //ClientSockStream.Write(message, 0, message.Length);
        //        return true;
        //    }
        //    return false;
        //}


        // old
        //public void SendFile(string path)
        //{
        //    Console.WriteLine("TESTING CONSOLE");

        //    if (!File.Exists(path))
        //    {
        //        throw new FileNotFoundException();
        //    }
        //    if (!IsConnected())
        //    {
        //        throw new Exception("Not connected");
        //    }
        //    byte[] bytes = File.ReadAllBytes(path);
        //    ClientStreamWriter.WriteLine(bytes.Length.ToString());

        //    ClientStreamWriter.Flush();

        //    string? response = ClientStreamReader.ReadLine();
        //    if (response != "OK")
        //    {
        //        throw new Exception("Connection lost during communication.");
        //    }
        //    ClientStreamWriter.WriteLine(Path.GetFileName(path));
        //    ClientStreamWriter.Flush();
        //    response = ClientStreamReader.ReadLine();
        //    if (response != "OK")
        //    {
        //        throw new Exception("Connection lost during communication.");
        //    }
        //    TcpClient.Client.SendFile(path);
        //    response = ClientStreamReader.ReadLine();
        //    if (response != "OK")
        //    {
        //        throw new Exception("Connection lost during communication.");
        //    }
        //    //ClientSockStream.Write(bytes, 0, bytes.Length);
        //}
    }
}
