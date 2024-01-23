using SchedulerServerApp.DBModule;
using SchedulerServerApp.ServerModule;
using SharedResources.Messages;
using System.Net.Sockets;
using System.Timers;
//using SchedulerServerApp.ServerModule;

namespace SchedulerServerApp.SchedulerModule;

public class SchedulerEngine
{
    public DBCommunication db { get; set; }
    public Server server { get; set; }

    // Task Assignment Timer
    public System.Timers.Timer TaskAssignTimer { get; set; }
    private const int TaskAssignTimerInterval = 10 * 1000;

    public SchedulerEngine(DBCommunication _db, Server _server)
    {
        db = _db;
        server = _server;
        TaskAssignTimer = new System.Timers.Timer(TaskAssignTimerInterval);
        SetTaskAssignTimer();
        Console.WriteLine("Scheduler engine running");
    }

    private void SetTaskAssignTimer()
    {
        TaskAssignTimer.Elapsed += new ElapsedEventHandler(
            OnTaskAssignTimer);
        TaskAssignTimer.AutoReset = true;
        TaskAssignTimer.Enabled = true;
        OnTaskAssignTimer(this, null);
    }
    
    async public void OnTaskAssignTimer(object? source, ElapsedEventArgs? e)
    {
        //Console.WriteLine("Assigning timer tick.");
        //await db.PrintAllTasks();

        while (true)
        {
            // Get highest priority task
            TaskMessage? task_msg = await db.GetHighestPriotityTask();
            if (task_msg == null)
            {
                Console.WriteLine("No waiting task.");
                break;
            }

            //Console.WriteLine($"HIGHEST PRIORITY TASK ID: {task.ID}");

            // Get free computer client
            string? free_client_ip = await db.GetFreeComputerClientAsync();
            if (free_client_ip == null)
            {
                Console.WriteLine("No free client.");
                break;
            }

            // Get computer communication handle
            TcpClient? free_client = server.FindClientHandleByIP(free_client_ip);
            if (free_client == null)
            {
                Console.WriteLine("Lost connection with free client.");
                break;
            }

            // Send task to scheduler client
            Console.WriteLine("Sending task to free client.");
            server.SendMessageToClient(free_client, task_msg);

            // Change status of a client machine
            db.UpdateClientMachineStatus(free_client_ip, "Connecting", true);  // TODO - try catch block

            // Change status of a task
            db.UpdateTaskStatus(task_msg.ID.ToString(), "InProgress", null);
        }
    }
}
