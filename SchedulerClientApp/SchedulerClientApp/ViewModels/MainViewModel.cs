using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using SchedulerClientApp.ClientModule;
using SchedulerClientApp.Services;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using SharedLibEnums;
using SharedLibMessages;
using SchedulerClientApp.TaskManager;

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
    
    // Macros
    public delegate void MacroDelegate(string message, bool endl = true, 
        bool date = true);

    // Tcp connection properties
    private Client Client { get; set; }
    private ComputationalTask? CurrentTask;
    private LogService LogService;
    private Timer? ReconnectingTimer;
    private Timer? StatusTimer;
    private MacroDelegate Log;

    public MainViewModel()
    {
        LogService = new LogService(this);
        Log = LogService.Log;
        Log("Scheduler client app started.");
        Client = new Client(LogService);
        InitHandlers();
        SetReconnectingTimer();
        SetStatusTimer();
        CreateTcpConnection();
        OnStatusTimer(null);
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
        Log("Connecting to a server...");
        try
        {
            Client.Connect("127.0.0.1", 1234);
        }
        catch (SocketException)
        {
            Log("Server is offline");
        }
        catch (Exception ex)
        {
            Log($"Exception: {ex.GetType().Name} - {ex.Message}");
        }

        if (Client is not null && Client.IsConnected())
        {
            Log("Connection successful");
            ClientStatus = ClientStatusEnum.Connected;
        }
        else
        {
            Log("Connection failed");
            ClientStatus = ClientStatusEnum.Disconnected;
        }
    }

    private void SetStatusTimer()
    {
        StatusTimer = new Timer(15000);
        StatusTimer.Elapsed += new ElapsedEventHandler(OnStatusTimer);
        StatusTimer.AutoReset = true;
        StatusTimer.Enabled = true;
    }

    private void OnStatusTimer(object? source, ElapsedEventArgs? e = null)
    {
        if (!Client.IsConnected())
        {
            Log("Server is offline.");
            ClientStatus = ClientStatusEnum.Disconnected;
            return;
        }
        else
        {
            //Log("Server is online -> sending...");
        }
        SendStatusMessage();
    }

    private async void SendStatusMessage()
    {
        await Task.Run(() =>
        {
            try
            {
                StatusMessage message = new StatusMessage(ClientStatus.ToString());
                Log($"Sending status message. Status: {ClientStatus}");
                Client?.SendMessage(message);
            }
            catch (Exception ex)
            {
                Log($"Exception: {ex.Message} {ex.GetType()}");
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
            Log("Auto-reconnecting: ");
            CreateTcpConnection();
        }
    }

    private async Task MoreDetailsButtonFunction()
    {
        if (!Client.IsConnected())
        {
            Log("Server is offline");
            ClientStatus = ClientStatusEnum.Disconnected;
            return;
        }

        await Task.Run(() =>
        {
            try
            {
                BaseMessage message = new BaseMessage();
                Log("Sending base message");
                Client?.SendMessage(message);
            }
            catch (Exception ex)
            {
                Log($"Exception: {ex.Message} {ex.GetType()}");
            }
        });
    }

    private async Task ReconnectButtonFunction()
    {
        if (ClientStatus != ClientStatusEnum.Disconnected)
        {
            await Task.Run(() =>
            {
                Client?.Disconnect();
            });
        }
        Log("Disconnected");
        ClientStatus = ClientStatusEnum.Disconnected;
        CreateTcpConnection();
    }
}
