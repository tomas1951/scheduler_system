using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using SchedulerClientApp.ClientModule;
using SchedulerClientApp.Services;
using SchedulerClientApp.TaskManager;
using SharedResources.Enums;
using SharedResources.Messages;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace SchedulerClientApp.ViewModels;

/// <summary>
/// <c>MainViewModel</c> class represents a middle layer between the code and
/// the User Interface. 
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // These properties are bound to the labels in User Interface
    // Note: Capital letters properties are auto-generated and using non-capital
    // variables in code will lead to errors.
    [ObservableProperty]
    private string serverConnectionLabel = "";
    [ObservableProperty]
    private ClientStatus clientStatusLabel = ClientStatus.Disconnected;
    [ObservableProperty]
    private string taskAssignedLabel = "";
    [ObservableProperty]
    private string taskStatusLabel = "";
    [ObservableProperty]
    private string operatingSystemLabel = "";
    [ObservableProperty]
    private string clusterLabel = "";
    [ObservableProperty]
    private string clientNameLabel = "";
    [ObservableProperty]
    private string clientIPLabel = "";
    [ObservableProperty]
    private string clientLogList = string.Empty;

    // Class holding local changeable copies of the ui labels
    public StatusParameters Status { get; set; } = new StatusParameters
    {
        ServerConnection = "offline",
        ClientStatus = ClientStatus.Disconnected,
        TaskAssigned = "no assigned task",
        TaskStatus = "no assigned task",
        OperatingSystem = "not recognised",
        Cluster = "note recognised",
        ClientName = "not recognised",
        ClientIP = "not recognised"
    };

    // Button handlers
    public ICommand? MoreDetailsButtonCommand { get; set; }
    public ICommand? ReconnectButtonCommand { get; set; }

    // Macro for simpler way of logging messages
    public delegate void MacroDelegate(string message, bool endl = true,
        bool date = true);
    private readonly MacroDelegate Log;

    // Time of timers in milliseconds
    private const int ReconnectingTimerInterval = 10000;
    private const int StatusTimerInterval = 15000;
    private const int UILabelsSyncTimerInterval = 1000;

    // Server info
    private const string ServerIP = "127.0.0.1";
    private const int Port = 1234;

    // Tcp connection properties
    private SchedulerClient Client { get; set; }
    private LogService LogService { get; set; }
    private Timer? ReconnectingTimer { get; set; }
    private Timer? StatusTimer { get; set; }
    private Timer? UILabelsSyncTimer { get; set; }

    public MainViewModel()
    {
        // Start up log service
        LogService = new LogService(this);
        Log = LogService.Log;
        Log("SCHEDULER SYSTEM CLIENT APP started.");
        
        // Catching unhandled exceptions
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException += 
            new UnhandledExceptionEventHandler(CatchUnhandledExceptions);

        // Start the Scheduler Client
        Log($"Connecting to a server {ServerIP}:{Port} ...");
        Client = new SchedulerClient(LogService);

        // Set up handlers and timers
        InitializeHandlers();
        SetUILabelsSyncTimer();
        SetReconnectingTimer();
        SetStatusTimer();
        // TODO: load client info
    }
    
    /***
    Handler functions
    ***/
    public void InitializeHandlers()
    {
        // More Detains Button Handler
        MoreDetailsButtonCommand = ReactiveCommand.Create(OnMoreDetailsButtonPressed);
        // Reconnect Button Handler
        ReconnectButtonCommand = ReactiveCommand.Create(OnReconnectButtonPressed);
    }

    /***
    UI labels sync (test)
    ***/
    public void SetUILabelsSyncTimer()
    {
        UILabelsSyncTimer = new Timer(UILabelsSyncTimerInterval);
        UILabelsSyncTimer.Elapsed += new ElapsedEventHandler(OnUILabelsSyncTimer2);
        UILabelsSyncTimer.Start();
    }

    // This is sync method for updating UI labels
    public void OnUILabelsSyncTimer2(object? source, ElapsedEventArgs? e = null)
    {
        ServerConnectionLabel = Status.ServerConnection;
        ClientStatusLabel = Status.ClientStatus;
        TaskAssignedLabel = Status.TaskAssigned;
        TaskStatusLabel = Status.TaskStatus;
        OperatingSystemLabel = Status.OperatingSystem;
        ClusterLabel = Status.Cluster;
        ClientIPLabel = Status.ClientIP;
    }
    
    /***
    Reconnecting Timer functions
    ***/
    private void SetReconnectingTimer()
    {
        ReconnectingTimer = new Timer(ReconnectingTimerInterval);
        ReconnectingTimer.Elapsed += new ElapsedEventHandler(OnReconnectingTimer);
        ReconnectingTimer.Start();
        OnReconnectingTimer(this, null);
    }

    private void OnReconnectingTimer(object? source, ElapsedEventArgs? e)
    {
        Status.ClientStatus = Client.GetConnectionStatus();
        if (Status.ClientStatus == ClientStatus.Disconnected)
        {
            Status.ClientStatus = ClientStatus.Reconnecting;
            Client.Connect(ServerIP, Port, Status);
        }
    }

    /***
    Status Timer functions
    ***/
    private void SetStatusTimer()
    {
        StatusTimer = new Timer(StatusTimerInterval);
        StatusTimer.Elapsed += new ElapsedEventHandler(OnStatusTimer);
        StatusTimer.Start();
    }

    private void OnStatusTimer(object? source, ElapsedEventArgs? e = null)
    {
        Status.ClientStatus = Client.GetConnectionStatus();
        if (Status.ClientStatus == ClientStatus.Connected)
        {
            Client.SendStatusMessage();
        }
    }

    /***
    BUTTON FUNCTIONS
    ***/
    // More Details button pressed function.
    private async Task OnMoreDetailsButtonPressed()
    {
        if (!Client.IsConnected())
        {
            Log("Server is offline");
            Status.ClientStatus = ClientStatus.Disconnected;
            return;
        }

        await Task.Run(() =>
        {
            Client.TestSendBaseMessage();
        });
    }

    // Reconnect button pressed function.
    private async Task OnReconnectButtonPressed()
    {
        if (Client.IsConnected())
        {
            await Task.Run(() =>
            {
                Client.Disconnect();
            });
        }
        Log("Disconnected");
    }

    // Exceptions catcher helper function.
    void CatchUnhandledExceptions(object sender, 
    UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception) args.ExceptionObject;
        Log($"MyHandler caught : {e.Message}");
        Log($"Runtime terminating: {args.IsTerminating}");

        // Note - try throwing testing exception to see how to keep the app running
        Client = new SchedulerClient(LogService);
    }
}
