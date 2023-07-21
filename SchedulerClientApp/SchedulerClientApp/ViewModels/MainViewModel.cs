using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace SchedulerClientApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string serverConnection = "offline";
    [ObservableProperty]
    private string clientStatus = "Idle";
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
    // Testing Timer
    private Timer? aTimer;
    // Button command handler
    public ICommand? MoreDetailsButtonCommand { get; set; }
    // Button command handler
    public ICommand? ReconnectButtonCommand { get; set; }

    public MainViewModel()
    {
        Init();

        SetTimer();

        // Testing code to change variable
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            ServerConnection = "online";
        });

        // Testing full text box
        ReceivedMessages += "Scheduler Client App started.\n";
    }

    public void Init()
    {
        // More Detains Button Handler
        MoreDetailsButtonCommand = ReactiveCommand.Create(MoreDetailsButtonFunction);
        // Reconnect Button Handler
        ReconnectButtonCommand = ReactiveCommand.Create(ReconnectButtonFunction);
        // Initialize Testing Timer
    }

    private void OnTimedEvent(object? source, ElapsedEventArgs e)
    {
        ReceivedMessages += (("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                          e.SignalTime) + "\n");
    }
    
    private void SetTimer()
    {
        // Create a timer with a two second interval.
        aTimer = new Timer(2000);
        // Hook up the Elapsed event for the timer. 
        aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    // Testing function
    public void Function(string message)
    {
        ReceivedMessages += (message + "\n");
    }

    private void MoreDetailsButtonFunction()
    {
        // Testing full text box
        ReceivedMessages += "More Details button pressed.\n";
    }

    private void ReconnectButtonFunction()
    {
        // Testing full text box
        ReceivedMessages += "Reconnect button pressed.\n";
    }
}
