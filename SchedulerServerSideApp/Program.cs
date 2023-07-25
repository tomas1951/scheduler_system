using System.Net.Sockets;
using System.Net;
using System.Text;

namespace SchedulerServerSideApp
{
    internal class Program
    {
        const int PORT_NO = 1234;
        const string SERVER_IP = "127.0.0.1";

        static void Main(string[] args)
        {
            //---listen at the specified IP and port no.---
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            Console.WriteLine("Listening...");
            listener.Start();
            while (true)
            {
                //---incoming client connected---
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected: {0}", client.Client.RemoteEndPoint?.ToString());

                //---get the incoming data through a network stream---
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                while (true)
                {
                    try
                    {
                        //---read incoming stream---
                        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                        if (bytesRead > 0)
                        {
                            //---convert the data received into a string---
                            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            Console.WriteLine("Received : " + dataReceived);

                            //---write back the text to the client---
                            Console.WriteLine("Sending back : " + dataReceived + "\n");
                            nwStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Exception: {0}", ex.Message);
                        break;
                    }
                }
                client.Close();
            }
            listener.Stop();
            Console.ReadLine();
        }
    }
}
