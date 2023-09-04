using SharedResources.Messages;
using System.Collections.Generic;
using System.Net.Sockets;
using SchedulerClientApp.Services;
using SharedResources.Enums;

namespace SchedulerClientApp.ClientModule;

/// <summary>
/// Interface <c>ISchedulerClient</c> defines functionality of a client                  
/// in the Scheduler System.
/// 
/// </summary>
public interface ISchedulerClient
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

    /// <summary>
    /// Creates a socket connection to address and port given by parameters.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    ClientStatus Connect(string message, int port);

    bool IsConnected();

    void Disconnect();

    void SendMessage(BaseMessage message);

    void SendingMethod();

    void ReceivingMethod();
}
