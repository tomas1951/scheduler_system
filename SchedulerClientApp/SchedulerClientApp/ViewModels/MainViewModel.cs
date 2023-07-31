using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using System;
using System.Timers;
using System.Windows.Input;
using SchedulerClientApp.Modules;
using System.Net.Sockets;
using System.Threading.Tasks;
using SchedulerClientApp.Resources;
using System.Text.Json;

namespace SchedulerClientApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // These properties are bound to the labels in User Interface
    // Note: 
    // Capital letters properties are auto-generated and using 
    // non-capital variables in code will lead to errors
    [ObservableProperty]
    private string serverConnection = "offline";
    [ObservableProperty]
    private string clientStatus = "idle...";
    [ObservableProperty]
    private bool taskAssigned = false;
    [ObservableProperty]
    private string taskStatus = "no assigned task";
    [ObservableProperty]
    private string operatingSystem = "not recognised";
    [ObservableProperty]
    private string cluster = "not recognised";
    [ObservableProperty]
    private string clientName = "not recognised";
    [ObservableProperty]
    private string clientIP = "not recognised";
    [ObservableProperty]
    private string _ReceivedMessages = string.Empty;
    // Button handlers
    public ICommand? MoreDetailsButtonCommand { get; set; }
    public ICommand? ReconnectButtonCommand { get; set; }
    // Tcp connection properties
    private Client? Client;
    private Timer? ReconnectingTimer;
    private Timer? StatusTimer;

    public MainViewModel()
    {
        ConsoleLog("Scheduler client app started.");
        InitHandlers();
        SetReconnectingTimer();
        SetStatusTimer();
        CreateTcpConnection();

        //TcpModuleInstance?.SendFile("C:\\Users\\Admin\\Desktop\\scheduler_system\\SchedulerClientApp\\Data\\input.txt");
    }

    public void InitHandlers()
    {
        // More Detains Button Handler
        MoreDetailsButtonCommand = ReactiveCommand.Create(MoreDetailsButtonFunction);
        // Reconnect Button Handler
        ReconnectButtonCommand = ReactiveCommand.Create(ReconnectButtonFunction);
    }

    private void CreateTcpConnection()
    {
        ConsoleLog("Connecting to a server...");
        try
        {
            //TcpModuleInstance = new Client("127.0.0.1", 1234);
            Client = new Client();
            Client.Connect("127.0.0.1", 1234);
        }
        catch (SocketException)
        {
            ConsoleLog("Server is offline.");
        }
        catch (Exception ex)
        {
            ConsoleLog(string.Format("Exception: {0} - {1}", ex.GetType().Name, ex.Message));
        }

        if (Client is not null && Client.IsConnected())
        {
            ConsoleLog("Connection successful");
            ServerConnection = "online";
        }
        else
        {
            ConsoleLog("Connection failed.\n");
            ServerConnection = "failed";
        }
    }

    private void SetStatusTimer()
    {
        StatusTimer = new Timer(20000);
        StatusTimer.Elapsed += new ElapsedEventHandler(OnStatusTimer);
        StatusTimer.AutoReset = true;
        StatusTimer.Enabled = true;
    }

    private void OnStatusTimer(object? source, ElapsedEventArgs e)
    {
        if (ServerConnection != "online")
        {
            return;
        }
        //string textToSend = DateTime.Now.ToString();
        //byte[] bytesToSend = Encoding.ASCII.GetBytes(textToSend);
        //ConsoleLog("Sending : " + textToSend);
        //if (TcpModuleInstance?.SendMessage(bytesToSend) == false)
        //{
        //    ConsoleLog("Server is offline.");
        //    ServerConnection = "offline";
        //}
    }

    private void SetReconnectingTimer()
    {
        ReconnectingTimer = new Timer(15000);
        ReconnectingTimer.Elapsed += new ElapsedEventHandler(OnReconnectingTimer);
        ReconnectingTimer.AutoReset = true;
        ReconnectingTimer.Enabled = true;
    }

    private void OnReconnectingTimer(object? source, ElapsedEventArgs e)
    {
        if (ServerConnection == "offline" || ServerConnection == "failed")
        {
            ServerConnection = "reconnecting...";
            ConsoleLog("Auto-reconnecting: ");
            CreateTcpConnection();
        }
    }

    private async Task MoreDetailsButtonFunction()
    {
        await Task.Run(() =>
        {
            ConsoleLog("Sending a message on other thread...");
            try
            {
                StatusMessage message = new StatusMessage("sending current status");
                ConsoleLog(string.Format("Sending: {0}", JsonSerializer.Serialize(message)));
                Client?.SendMessage(message);

                //TcpModuleInstance?.SendFile(
                    //"C:\\Users\\Admin\\Desktop\\scheduler_system\\SchedulerClientApp\\Data\\input.txt");
            }
            catch (Exception ex)
            {
                ConsoleLog(string.Format("Exception: {0} {1}",
                    ex.Message, ex.GetType().ToString()));
            }
            ConsoleLog("Message sent.");
        });

        //await Task.Run(() =>
        //{
        //    ConsoleLog("Creating Base Message.");
        //    StatusMessage message = new StatusMessage("status", "idle");
        //    ConsoleLog("Json message:");
        //    ConsoleLog(message.GetJsonString());
        //    ConsoleLog("");
        //});
    }

    private async Task ReconnectButtonFunction()
    {
        await Task.Run(() =>
        {
            Client?.Disconnect();
        });
        ConsoleLog("Disconnected.");
        CreateTcpConnection();
    }

    private void ConsoleLog(string message, bool endl = true, bool date = true)
    {
        if (endl)
        {
            message += "\n";
        }
        if (date)
        {
            DateTime currentDateTime = DateTime.Now;
            string formattedDate = currentDateTime.ToString("MM/dd/yyyy HH:mm:ss");
            message = (formattedDate + "> " + message);
        }
        ReceivedMessages += message;
    }
}
