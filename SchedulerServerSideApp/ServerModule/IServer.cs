using SharedResources.Messages;
using System.Net.Sockets;
using System.Timers;

namespace SchedulerServerApp.ServerModule;

/// <summary>
/// Interface <c>IServer</c> defines functionality of a server class
/// in a scheduling system.
/// 
/// </summary>
public interface IServer
{
    /// <summary>
    /// Property defining server port.
    /// </summary>
    int Port { get; set; }

    /// <summary>
    /// Flag property defining whether server is already running.
    /// </summary>
    // NOTE: MIGHT BE REMOVED
    bool IsStarted { get; set; }

    /// <summary>
    /// TCP listener property for communication functionality.
    /// </summary>
    TcpListener Listener { get; set; }

    /// <summary>
    /// List of current connected clients on the server.
    /// </summary>
    List<TcpClient> ConnectedClients { get; set; }

    /// <summary>
    /// Property defining a timer that listens for new clients.
    /// </summary>
    System.Timers.Timer ListeningTimer { get; set; }

    //void Start();

    void IsOnline();

    void OnListeningTimer(object? source, ElapsedEventArgs e);

    void ListenForNewClients();

    void Stop();

    void OnClientConnected(TcpClient client);

    void HandleClient(TcpClient client);

    void OnMessageReceived(object? sender, MessageFromClient message);

    void SendMessageToClient(TcpClient client, BaseMessage message);
    void DisconnectClient(TcpClient client);
}
