using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using SchedulerClientApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SchedulerClientApp.ViewModels;

public class MainViewModel : ViewModelBase, INotifyPropertyChanged
{
    private string _serverConnection = "offline";
    private string _clientStatus = "Idle";
    private bool _taskAssigned = false;
    private string _taskStatus = "no assigned task";
    private string _operatingSystem = "not recognised";
    private string _cluster = "not recognised";
    private string _clientName = "not recognised";
    private string _clientIP = "not recognised";

    public MainViewModel()
    {
        // chatgpt
        //Messages = new ObservableCollection<string>();
        //Messages.CollectionChanged += Messages_CollectionChanged;

        //_window = window;

        // Button Handler to activate function
         ButtonTestFunctionCommand = ReactiveCommand.Create(ButtonTestFunction);

        // Testing code to change variable
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            ServerConnection = "online";
        });

        // Testing iterable text box
        //AddMessage("First message.");
        
        // Testing full text box
        ReceivedMessages += "New message \n";
    }

    public string ServerConnection
    {
        get => _serverConnection;
        set
        {
            if (_serverConnection == value)
                return;

            _serverConnection = value;
            OnPropertyChanged(nameof(ServerConnection));
        }
    }

    public string ClientStatus
    {
        get => _clientStatus;
        set
        {
            if (_clientStatus == value)
                return;

            _clientStatus = value;
            OnPropertyChanged(nameof(ClientStatus));
        }
    }
    
    public bool TaskAssigned
    {
        get => _taskAssigned;
        set
        {
            if (_taskAssigned == value)
                return;

            _taskAssigned = value;
            OnPropertyChanged(nameof(TaskAssigned));
        }
    }
    
    public string TaskStatus
    {
        get => _taskStatus;
        set
        {
            if (_taskStatus == value)
                return;

            _taskStatus = value;
            OnPropertyChanged(nameof(TaskStatus));
        }
    }
    
    public string OperatingSystem
    {
        get => _operatingSystem;
        set
        {
            if (_operatingSystem == value)
                return;

            _operatingSystem = value;
            OnPropertyChanged(nameof(OperatingSystem));
        }
    }
    
    public string Cluster
    {
        get => _cluster;
        set
        {
            if (_cluster == value)
                return;

            _cluster = value;
            OnPropertyChanged(nameof(Cluster));
        }
    }
    
    public string ClientName
    {
        get => _clientName;
        set
        {
            if (_clientName == value)
                return;

            _clientName = value;
            OnPropertyChanged(nameof(ClientName));
        }
    }
    
    public string ClientIP
    {
        get => _clientIP;
        set
        {
            if (_clientIP == value)
                return;

            _clientIP = value;
            OnPropertyChanged(nameof(ClientIP));
        }
    }

    public event PropertyChangedEventHandler ? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    //protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    //{
    //    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
    //    field = value;
    //    OnPropertyChanged(propertyName);
    //    return true;
    //}

    // Button command handler
    public ICommand ? ButtonTestFunctionCommand { get; }

    private void ButtonTestFunction()
    {
        // Test changing a variable
        ClientStatus = "connecting";

        // Testing full text box
        ReceivedMessages += "New message \n";

        //AddMessage("New message \n");

        //Thread.Sleep(10);
        //Dispatcher.UIThread.InvokeAsync(_window.ScrollTextToEnd);
    }

    // Testing full text variable
    private string _ReceivedMessages = string.Empty;

    public string ReceivedMessages
    {
        get => _ReceivedMessages;
        set
        {
            if (_ReceivedMessages == value)
                return;
            _ReceivedMessages = value;
            OnPropertyChanged(nameof(ReceivedMessages));
        }
    }

    /// <summary>
    ///  ChatGPS solution for autoscrolling
    /// </summary>
    //private ObservableCollection<string> _messages = new ObservableCollection<string>();
    //public ObservableCollection<string> Messages
    //{
    //    get => _messages;
    //    set
    //    {
    //        if (_messages != value)
    //        {
    //            _messages = value;
    //            OnPropertyChanged();
    //        }
    //    }
    //}

    //private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //{
    //    MessageAdded?.Invoke(this, EventArgs.Empty);
    //}

    //public event EventHandler ? MessageAdded;

    //public void AddMessage(string message)
    //{
    //    Messages.Add(message);
    //}

}
