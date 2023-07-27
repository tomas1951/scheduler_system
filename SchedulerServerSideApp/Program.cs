using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Reflection;

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
                //byte[] buffer = new byte[8192];
                int fileCounter = 0;

                while (true)
                {
                    fileCounter += 1;    
                    string outputFilePath = @"C:\Users\Admin\Desktop\scheduler_system\SchedulerServerSideApp\Data\" + "file" + fileCounter.ToString() + ".txt";

                    try
                    {
                        StreamReader reader = new StreamReader(client.GetStream());
                        StreamWriter writer = new StreamWriter(client.GetStream());

                        string? fileSize = reader.ReadLine();
                        int length = Convert.ToInt32(fileSize);
                        Console.WriteLine("Received data: {0}", fileSize);
                        writer.Write("OK");

                        string? cmdFileName = reader.ReadLine();
                        Console.WriteLine("Received data: {0}", cmdFileName);
                        writer.Write("OK");

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

                        using (FileStream fStream = new FileStream(Path.GetFileName(cmdFileName), FileMode.Create))
                        {
                            fStream.Write(buffer, 0, buffer.Length);
                            fStream.Flush();
                            fStream.Close();
                            Console.WriteLine("File {0} of size {1} received.", cmdFileName, length);
                        }

                        //int bytesRead;
                        //FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);

                        //while ((bytesRead = nwStream.Read(buffer, 0, buffer.Length)) > 0)
                        //{
                        //    Console.WriteLine("Received data: " + bytesRead);
                        //    outputFileStream.Write(buffer, 0, bytesRead);
                        //}

                        ////---read incoming stream---
                        //int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                        //if (bytesRead > 0)
                        //{
                        //    //---convert the data received into a string---
                        //    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        //    Console.WriteLine("Received : " + dataReceived);

                        //    //---write back the text to the client---
                        //    Console.WriteLine("Sending back : " + dataReceived + "\n");
                        //    nwStream.Write(buffer, 0, bytesRead);
                        //}
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
            listener.Stop();
            Console.ReadLine();
        }
    }
}
