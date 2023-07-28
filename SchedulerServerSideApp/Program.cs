using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Reflection;
using System.Collections;

namespace SchedulerServerSideApp
{
    internal class Program
    {
        const int PORT_NO = 1234;
        const string SERVER_IP = "127.0.0.1";

        static void Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("DEBUG MODE");
#endif
            Console.WriteLine("Server started.");
            //---listen at the specified IP and port no.---
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);

            while (true)
            {
                Console.WriteLine("Listening...");
                listener.Start();
                //---incoming client connected---
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected: {0}", client.Client.RemoteEndPoint?.ToString());
                //---get the incoming data through a network stream---
                NetworkStream nwStream = client.GetStream();

                while (true)
                {
                    try
                    {
                        StreamReader reader = new StreamReader(client.GetStream());
                        StreamWriter writer = new StreamWriter(client.GetStream());

                        string? fileSize = reader.ReadLine();
                        int length = Convert.ToInt32(fileSize);
                        Console.WriteLine("Received file size: {0}", fileSize);
                        writer.WriteLine("OK");
                        writer.Flush();
                        Console.WriteLine("Sending confirmation");

                        string? cmdFileName = reader.ReadLine();
                        Console.WriteLine("Received file name: {0}", cmdFileName);
                        writer.WriteLine("OK");
                        writer.Flush();
                        Console.WriteLine("Sending confirmation");

                        byte[] buffer = new byte[length];
                        int received = 0;
                        int read = 0;
                        int size = 1024;
                        int remaining = 0;

                        while (received < length)
                        {
                            remaining = length - received;
                            if (remaining < size)
                            {
                                size = remaining;
                            }

                            read = client.GetStream().Read(buffer, received, size);
                            received += read;
                        }

                        string path = @"C:\Users\Admin\Desktop\scheduler_system\SchedulerServerSideApp\Data\" + cmdFileName;

                        using (FileStream fStream = new FileStream(path, FileMode.Create))
                        {
                            fStream.Write(buffer, 0, buffer.Length);
                            fStream.Flush();
                            fStream.Close();
                            Console.WriteLine("File {0} of size {1} received.", cmdFileName, length);
                        }

                        Console.WriteLine("Received file: {0}", Encoding.UTF8.GetString(buffer));
                        writer.WriteLine("OK");
                        writer.Flush();
                        Console.WriteLine("Sending confirmation");
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Exception: {0} {1}", ex.Message, ex.GetType().Name);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception: {0} {1}", ex.Message, ex.GetType().Name);
                        break;
                    }
                }
                Console.WriteLine("Client {0} left the server", client.Client.RemoteEndPoint?.ToString());
                client.Close();
            }
        }
    }
}
