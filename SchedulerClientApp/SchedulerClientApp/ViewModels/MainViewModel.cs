using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
    
    public MainViewModel()
    {
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            ServerConnection = "online";
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
