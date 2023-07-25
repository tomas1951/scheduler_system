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
        public StreamReader ClientStreamReader;
        public StreamWriter ClientStreamWriter;

        public TcpModule(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;

            TcpConnection = new TcpClient(Hostname, Port);
            NetworkStream clientSockStream = TcpConnection.GetStream();
            ClientStreamReader = new StreamReader(clientSockStream);
            ClientStreamWriter = new StreamWriter(clientSockStream);
        }

        public bool IsConnected()
        {
            return TcpConnection.Connected;
        }
    }
}
