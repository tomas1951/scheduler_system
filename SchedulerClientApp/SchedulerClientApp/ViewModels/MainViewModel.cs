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
    private string serverConnection = "deprecated";
    [ObservableProperty]
    private ClientStatus clientStatus = ClientStatus.Disconnected;
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

    // Local changeable copies of the ui labels


    // Button handlers
    public ICommand? MoreDetailsButtonCommand { get; set; }
    public ICommand? ReconnectButtonCommand { get; set; }

    // Macro for simpler way of logging messages
    public delegate void MacroDelegate(string message, bool endl = true,
        bool date = true);
    private readonly MacroDelegate Log;

    // Time of timers in milliseconds
    private const int ReconnectingTimerInterval = 5000;
    private const int StatusTimerInterval = 10000;
    private const int UILabelsSyncTimerInterval = 1000;

    // Server info
    private const string ServerIP = "127.0.0.1";
    private const int Port = 1234;

    // Tcp connection properties
    private SchedulerClient Client { get; set; }
    private LogService LogService;
    private Timer? ReconnectingTimer;
    private Timer? StatusTimer;
    private Timer? UILabelsSyncTimer;

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
        SetReconnectingTimer();
        SetStatusTimer();
        //SetUILabelsSyncTimer();
    }

    /***
    UI labels sync (test)
    ***/
    public void SetUILabelsSyncTimer()
    {
        UILabelsSyncTimer = new Timer(UILabelsSyncTimerInterval);
        UILabelsSyncTimer.Elapsed += new ElapsedEventHandler(OnUILabelsSyncTimer2);
        UILabelsSyncTimer.AutoReset = true;
        UILabelsSyncTimer.Enabled = true;
    }

    // This is sync method for updating UI labels
    public void OnUILabelsSyncTimer2(object? source, ElapsedEventArgs? e = null)
    {

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
    Status Timer functions
    ***/
    private void SetStatusTimer()
    {
        StatusTimer = new Timer(StatusTimerInterval);
        StatusTimer.Elapsed += new ElapsedEventHandler(OnStatusTimer);
        StatusTimer.AutoReset = true;
        StatusTimer.Enabled = true;
    }

    private void OnStatusTimer(object? source, ElapsedEventArgs? e = null)
    {
        ClientStatus = Client.GetConnectionStatus();
        if (ClientStatus == ClientStatus.Connected)
        {
            SendStatusMessage();
        }
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

    /***
    Reconnecting Timer functions
    ***/
    private void SetReconnectingTimer()
    {
        ReconnectingTimer = new Timer(ReconnectingTimerInterval);
        ReconnectingTimer.Elapsed += new ElapsedEventHandler(OnReconnectingTimer);
        ReconnectingTimer.AutoReset = true;
        ReconnectingTimer.Enabled = true;
    }

    private void OnReconnectingTimer(object? source, ElapsedEventArgs e)
    {
        ClientStatus = Client.GetConnectionStatus();
        if (ClientStatus == ClientStatus.Disconnected)
        {
            ClientStatus = ClientStatus.Reconnecting;
            Log("Auto-reconnecting... ");
            ClientStatus = Client.Connect(ServerIP, Port);
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
            ClientStatus = ClientStatus.Disconnected;
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
