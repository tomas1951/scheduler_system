using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Messages;
using Enums;

namespace SchedulerClientApp.ClientModule
{
    public class Client
    {
        public TcpClient TcpClient = new TcpClient();
        public List<BaseMessage> MessageQueue = new List<BaseMessage>();
        public ClientStatusEnum Status = ClientStatusEnum.Disconnected;

        public void Connect(string address, int port)
        {
            TcpClient = new TcpClient();
            TcpClient.Connect(address, port);
            Status = ClientStatusEnum.Connected;
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
            Status = ClientStatusEnum.Disconnected;
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
            while (Status != ClientStatusEnum.Disconnected)
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
                    }
                    MessageQueue.Remove(message);
                }
                Thread.Sleep(30);
            }
        }

        public void ReceivingMethod()
        {

        }
    }
}
