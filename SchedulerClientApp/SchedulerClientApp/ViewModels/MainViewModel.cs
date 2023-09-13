﻿using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using SchedulerClientApp.ClientModule;
using SchedulerClientApp.Services;
using SharedResources.Enums;
using System;
using System.IO;
using System.Net;
using System.Threading;
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
        TaskAssigned = "[no assigned task]",
        TaskStatus = "[no assigned task]",
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
    private const int ReconnectingTimerInterval = 15000;
    private const int StatusTimerInterval = 15000;
    private const int UILabelsSyncTimerInterval = 1000;

    // Server info
    //private const string ServerIP = "127.0.0.1"; // local host
    //private const string ServerIP = "172.20.1.45";
    //private const int Port = 1234;
    private string ServerIP { get; set; } = "";
    private int Port { get; set; }

    // Congig info
    private const string ConfigPath = "C:\\Users\\Admin\\Desktop\\scheduler_system\\" +
        "SchedulerClientApp\\client_config.txt";

    // Tcp connection properties
    private SchedulerClient Client { get; set; }
    private LogService LogService { get; set; }
    private System.Timers.Timer? ReconnectingTimer { get; set; }
    private System.Timers.Timer? StatusTimer { get; set; }
    private System.Timers.Timer? UILabelsSyncTimer { get; set; }

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


    /***
    Load Labels
    ***/
    // Loads a client parameters from the config file.
    public bool LoadClientConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            Log($"File {ConfigPath} doesn't exist or user doesnt have permission" +
                $"to read the file.");
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
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
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

                    Log($"Key: {key}, Value: {value}");

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
}
