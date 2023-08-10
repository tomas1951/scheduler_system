using System.Net.Sockets;

namespace SchedulerServerSideApp
{
    public class MessageFromClient : EventArgs
    {
        public TcpClient Client { get; }
        public string Content { get ; }

        public MessageFromClient(TcpClient client, string content)
        {
            Client = client;
            Content = content;
        }
    }
}
