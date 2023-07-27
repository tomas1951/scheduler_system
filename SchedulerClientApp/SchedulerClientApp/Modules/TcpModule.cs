using Svg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerClientApp.Modules
{
    public class TcpModule
    {
        public string Hostname;
        public int Port;
        public TcpClient TcpConnection;
        public NetworkStream ClientSockStream;
        public StreamReader ClientStreamReader;
        public StreamWriter ClientStreamWriter;

        public TcpModule(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;

            TcpConnection = new TcpClient(Hostname, Port);
            ClientSockStream = TcpConnection.GetStream();
            ClientStreamReader = new StreamReader(ClientSockStream);
            ClientStreamWriter = new StreamWriter(ClientSockStream);
        }

        public bool IsConnected()
        {
            return TcpConnection.Connected;
        }

        public void Disconnect()
        {
            TcpConnection.GetStream().Close();
            TcpConnection.Close();
        }

        public bool SendMessage(byte[] message)
        {
            if (IsConnected())
            {
                //ClientStreamWriter.WriteLine(message);
                ClientSockStream.Write(message, 0, message.Length);
                return true;
            }
            return false;
        }

        public bool SendFile(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            if (!IsConnected())
            {
                return false;
            }           
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                ClientStreamWriter.WriteLine(bytes.Length.ToString());
                ClientStreamWriter.Flush();

                string ? response = ClientStreamReader.ReadLine();
                if (response != "OK")
                    return false;
                ClientStreamWriter.WriteLine(path);
                ClientStreamWriter.Flush();
                response = ClientStreamReader.ReadLine();
                if (response != "OK")
                    return false;

                TcpConnection.Client.SendFile(path);
                response = ClientStreamReader.ReadLine();
                if (response != "OK")
                    return false;
                //ClientSockStream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
