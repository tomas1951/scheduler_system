using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using SchedulerClientApp.ClientModule;
using SchedulerClientApp.Services;
using SharedResources.Enums;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Timer = System.Timers.Timer;

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
    private string serverConnectionLabel = string.Empty;
    [ObservableProperty]
    private ClientStatus clientStatusLabel = ClientStatus.Disconnected;
    [ObservableProperty]
    private string taskAssignedLabel = string.Empty;
    [ObservableProperty]
    private SchedulerTaskStatus taskStatusLabel = SchedulerTaskStatus.NoAssignedTask;
    [ObservableProperty]
    private string operatingSystemLabel = string.Empty;
    [ObservableProperty]
    private string clusterLabel = string.Empty;
    [ObservableProperty]
    private string clientNameLabel = string.Empty;
    [ObservableProperty]
    private string clientIPLabel = string.Empty;
    [ObservableProperty]
    private string clientLogList = string.Empty;
    [ObservableProperty]
    private string statusLabel = string.Empty;

    // Class holding local changeable copies of the ui labels
    public StatusParameters Status { get; set; } = new StatusParameters
    {
        ServerConnection = "offline",
        ClientStatus = ClientStatus.Disconnected,
        TaskAssigned = "no",
        TaskStatus = SchedulerTaskStatus.NoAssignedTask,
        OperatingSystem = "[not recognised]",
        Cluster = "[note recognised]",
        ClientName = "[not recognised]",
        ClientIP = "[not recognised]"
    };

    // Button handlers
    public ICommand? MoreDetailsButtonCommand { get; set; }
    public ICommand? ReconnectButtonCommand { get; set; }

    // Macro for simpler way of logging messages
    public delegate void MacroDelegate(string message, bool endl = true,
        bool date = true);
    private readonly MacroDelegate Log;

    // Time of timers in milliseconds
    private const int ReconnectingTimerInterval = 15 * 1000;
    private const int StatusTimerInterval = 15 * 1000;
    private const int UILabelsSyncTimerInterval = 1 * 1000;

    // Server info
    private string ServerIP { get; set; } = "";
    private int Port { get; set; }

    // Congig info
    private const string ConfigPath = "client_config.ini";

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
        Log($"Connecting to a server...");
        Client = new SchedulerClient(LogService);

        // Set up handlers and timers
        InitializeHandlers();
        SetUILabelsSyncTimer();
        SetReconnectingTimer();
        SetStatusTimer();
    }


    #region Handler functions

    public void InitializeHandlers()
    {
        // More Detains Button Handler
        MoreDetailsButtonCommand = ReactiveCommand.Create(OnMoreDetailsButtonPressed);
        // Reconnect Button Handler
        ReconnectButtonCommand = ReactiveCommand.Create(OnReconnectButtonPressed);
    }

    #endregion Handler functions


    #region UI labels sync (test)
    
    public void SetUILabelsSyncTimer()
    {
        UILabelsSyncTimer = new Timer(UILabelsSyncTimerInterval);
        UILabelsSyncTimer.Elapsed += new ElapsedEventHandler(OnUILabelsSyncTimer);
        UILabelsSyncTimer.Start();
    }

    // This is sync method for updating UI labels
    public void OnUILabelsSyncTimer(object? source, ElapsedEventArgs? e = null)
    {
        if (Client.CurrentTask is not null)
        {
            Status.TaskAssigned = "yes";
            Status.TaskStatus = Client.CurrentTask.Status;
        }
        else
        {
            Status.TaskAssigned = "no";
            Status.TaskStatus = SchedulerTaskStatus.NoAssignedTask;
        }

        ServerConnectionLabel = Status.ServerConnection;
        ClientStatusLabel = Status.ClientStatus;
        TaskAssignedLabel = Status.TaskAssigned;
        TaskStatusLabel = Status.TaskStatus;
        OperatingSystemLabel = Status.OperatingSystem;
        ClusterLabel = Status.Cluster;
        ClientNameLabel = Status.ClientName;
        ClientIPLabel = Status.ClientIP;
    }

    #endregion UI labels sync (test)


    #region Reconnecting Timer functions

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

        if (ServerIP == "" && !LoadClientConfig())
        {
            return;
        }

        if (Status.ClientStatus == ClientStatus.Disconnected)
        {
            Status.ClientStatus = ClientStatus.Reconnecting;
            Client.Connect(ServerIP, Port, Status);
        }
    }

    #endregion Reconnecting Timer functions


    #region Status Timer functions

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
    
    #endregion Status Timer functions


    #region BUTTON FUNCTIONS

    // More Details button pressed function.
    private async Task OnMoreDetailsButtonPressed()
    {
        if (!Client.IsConnected())
        {
            Log("server is offline");
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
        Exception e = (Exception)args.ExceptionObject;
        Log($"MyHandler caught : {e.Message}");
        Log($"Runtime terminating: {args.IsTerminating}");

        // Note - try throwing testing exception to see how to keep the app running
        Client = new SchedulerClient(LogService);
    }

    #endregion BUTTON FUNCTIONS


    #region Load Labels

    // Loads a client parameters from the config file.
    public bool LoadClientConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            Log($"Error in reading config file {ConfigPath}.");
            return false;
        }

        try
        {
            using (StreamReader sr = new StreamReader(ConfigPath))
            {
                string? line;

                while ((line = sr.ReadLine()) != null)
                {
                    // Ignore lines starting with '#' or lines that are blank
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                    {
                        continue;
                    }
                    // Parse lines in "key = value" format
                    string[] parts = line.Split(new[] { '=' }, 2);
                    if (parts.Length != 2)
                    {
                        Log($"Config file error on line: {line}");
                        return false;
                    }
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    //Log($"Key: {key}, Value: {value}");

                    switch (key)
                    {
                        case "ServerIP":
                            ServerIP = value;
                            break;
                        case "Port":
                            Port = int.Parse(value);
                            break;
                        case "OperatingSystem":
                            Status.OperatingSystem = value;
                            break;
                        case "Cluster":
                            Status.Cluster = value;
                            break;
                        case "ClientName":
                            Status.ClientName = value;
                            break;
                        default:
                            Log($"Config error: parameter {key} not recognised.");
                            return false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log($"Config file exception: {e.Message}");
            return false;
        }
        // IP address and port number validation check
        if (!IPAddress.TryParse(ServerIP, out _) || !(Port > 0 && Port <= 65535))
        {
            Log($"Config error: ip address {ServerIP} or port number {Port} is" +
                $" not valid.");
            return false;
        }
        return true;
    }

    #endregion Load Labels
}
