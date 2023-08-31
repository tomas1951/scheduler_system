using SharedResources.Messages;
using System.Collections.Generic;
using System.Net.Sockets;
using SchedulerClientApp.Services;

namespace SchedulerClientApp.ClientModule;

/// <summary>
/// Interface <c>ISchedulerClient</c> defines functionality of a client                  
/// in the Scheduler System.
/// 
/// </summary>
public interface ISchedulerTcpClient
{
    /// <summary>
    /// Instance of a <c>TcpClient</c> class for tcp communication.
    /// </summary>
    TcpClient TcpClient { get; set; }

    // Might be deleted later
    List<BaseMessage> MessageQueue { get; set; }

    /// <summary>
    /// <c>LogService</c> writes messages both into a graphic console and 
    /// into a log file.
    /// </summary>
    LogService LogService { get; set; }

    void Connect(string message, int port);
    
    bool IsConnected();

    void Disconnect();

    void SendMessage(BaseMessage message);

    void SendingMethod();

    void ReceivingMethod();
}
