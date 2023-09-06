using SharedResources.Messages;
using System.Collections.Generic;
using System.Net.Sockets;
using SharedResources.Enums;
using SchedulerClientApp.Services;
using SchedulerClientApp.TaskManager;

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
    /// This property holds current task that is assigned to the scheduler client.
    /// </summary>
    SchedulerTask? CurrentTask { get; set; }

    /// <summary>
    /// Creates a socket connection to address and port given by parameters.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    ClientStatus Connect(string message, int port);

    /// <summary>
    /// Returns true, if connection with the server is active. Returns false otherwise.
    /// </summary>
    /// <returns></returns>
    bool IsConnected();

    /// <summary>
    /// Closes the connection with the server.
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Adds a message to the MessageQueue to be sent.
    /// </summary>
    /// <param name="message"></param>
    void SendMessage(BaseMessage message);

    /// <summary>
    /// Sends a message to the server if there is some message waiting in MessageQueue.
    /// This method runs in its own thread.
    /// </summary>
    void SendingMethod();

    /// <summary>
    /// Listens for a incomming messages.
    /// This method runs in its own thread.
    /// </summary>
    void ReceivingMethod();

    /// <summary>
    /// Prints incomming Status message into log.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="message"></param>
    void PrintMessage(TcpClient client, StatusMessage message);

    /// <summary>
    /// Prints incomming Base message into log.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="message"></param>
    void PrintMessage(TcpClient client, BaseMessage message);

    /// <summary>
    /// Returns client ip address for given client class argument.
    /// If client is null, function returns empty string. 
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    string GetClientIP(TcpClient client);
}
