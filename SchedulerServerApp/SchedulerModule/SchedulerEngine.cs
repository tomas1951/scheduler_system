using SchedulerServerApp.DBModule;
using System.Timers;
//using SchedulerServerApp.ServerModule;

namespace SchedulerServerApp.SchedulerModule;

public class SchedulerEngine
{
    public DBCommunication db { get; set; }

    // Task Assignment Timer
    public System.Timers.Timer TaskAssignTimer { get; set; }
    private const int TaskAssignTimerInterval = 10 * 1000;

    public SchedulerEngine(DBCommunication dBCommunication)
    {
        db = dBCommunication;
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
        await db.PrintAllTasks();

        while (true)
        {
            // Get highest priority task
            //Console.WriteLine("Sending task to free client.");
            
            

            // Get free computer client

            break;
        }
    }
}
  