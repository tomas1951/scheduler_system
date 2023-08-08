using System.Net.Sockets;
using System.Text;
using System.Timers;
using Messages;

namespace SchedulerServerSideApp
{
    public class Server
    {
        private int Port;
        private bool IsStarted = false;
        private TcpListener? Listener;
        private List<Receiver> Receivers = new List<Receiver>();
        // Thread signal
        public static ManualResetEvent tcpClientConnected = 
            new ManualResetEvent(false);
        // Listening timer
        private System.Timers.Timer ListeningTimer;
        // Handlers
        public static event EventHandler<string>? OnMessageReceived;

        public Server(int port)
        {
            Port = port;

            OnMessageReceived += HandleMessageReceived;
        }

        public void Start()
        {
            if (IsStarted)
            {
                return;
            }

            Listener = new TcpListener(System.Net.IPAddress.Any, Port);
            Listener.Start();
            IsStarted = true;

            SetListeningTimer();
            ListenForNewClients();
        }

        private void SetListeningTimer()
        {
            ListeningTimer = new System.Timers.Timer(30000);
            ListeningTimer.Elapsed += new ElapsedEventHandler(OnListeningTimer);
            ListeningTimer.AutoReset = true;
            ListeningTimer.Enabled = true;
        }

        private void OnListeningTimer(object? source, ElapsedEventArgs e)
        {
            ListenForNewClients();
        }

        private  async void ListenForNewClients()
        {
            if (Listener is not null)
            {
                Console.WriteLine("Listening for new clients.");
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

        private void OnClientConnected(TcpClient client)
        {
            Console.WriteLine("Client connected: {0}", 
                client.Client.RemoteEndPoint?.ToString());
            Thread clientThread = new Thread(
                () => HandleClient(client) 
            );
            clientThread.Start();
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    // Raise the OnMessageReceived event
                    OnMessageReceived?.Invoke(client, message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Console.WriteLine("Client {0} left the server. (Unable to read data)",
                    GetClientIP(client));
                client.Close();
                ListenForNewClients();
            }
        }

        private static void HandleMessageReceived(object sender, string message)
        {
            Console.WriteLine($"Client {GetClientIP((TcpClient)sender)} sends raw message: {message}");

        }

        private void PrintMessage(TcpClient client, StatusMessage message)
        {
            Console.WriteLine(string.Format("Host: {0}, Status: {1}", GetClientIP(client), message.CurrentStatus));
        }

        private static string GetClientIP(TcpClient client)
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

    }
}
