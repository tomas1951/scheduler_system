using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using System;
using System.Timers;
using System.Windows.Input;
using System.Net.Sockets;
using System.Threading.Tasks;
using Messages;
using SchedulerClientApp.ClientModule;
using Enums;

namespace SchedulerClientApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // These properties are bound to the labels in User Interface
    // Note: 
    // Capital letters properties are auto-generated and using 
    // non-capital variables in code will lead to errors
    [ObservableProperty]
    private string serverConnection = "deprecated";
    [ObservableProperty]
    private ClientStatusEnum clientStatus = ClientStatusEnum.Disconnected;
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
        SendStatusMessage();
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
            ClientStatus = ClientStatusEnum.Connected;
        }
        else
        {
            ConsoleLog("Connection failed.\n");
            ClientStatus = ClientStatusEnum.Disconnected;
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
        SendStatusMessage();
    }

    private async void SendStatusMessage()
    {
        if (ClientStatus != ClientStatusEnum.Connected)
        {
            return;
        }

        await Task.Run(() =>
        {
            try
            {
                StatusMessage message = new StatusMessage(ClientStatus.ToString());
                ConsoleLog(string.Format("Sending status message. Status: {0}", ClientStatus.ToString()));
                Client?.SendMessage(message);
            }
            catch (Exception ex)
            {
                ConsoleLog(string.Format("Exception: {0} {1}",
                    ex.Message, ex.GetType().ToString()));
            }
        });
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
        if (ClientStatus == ClientStatusEnum.Disconnected)
        {
            ClientStatus = ClientStatusEnum.Reconnecting;
            ConsoleLog("Auto-reconnecting: ");
            CreateTcpConnection();
        }
    }

    private async Task MoreDetailsButtonFunction()
    {

    }

    private async Task ReconnectButtonFunction()
    {
        await Task.Run(() =>
        {
            Client?.Disconnect();
        });
        ConsoleLog("Disconnected.");
        ClientStatus = ClientStatusEnum.Disconnected;
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
