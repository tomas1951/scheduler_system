using SchedulerServerSideApp.ServerModule;

namespace SchedulerServerSideApp;

internal class Program
{
    private static event EventHandler<string>? ReceivedCommandHandler;
    const int PORT_NO = 1234;

    static void Main(string[] args)
    {
        // Unhandled exceptions catcher
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException += 
            new UnhandledExceptionEventHandler(CatchUnhandledExceptions);
        
        // Handler for console commands
        ReceivedCommandHandler += OnReceivedCommand;
        Thread commandsThead = new Thread(ReadConsoleCommands);
        commandsThead.Start();

        //Start the server
        Server Server = new Server(PORT_NO);

        // Start the scheduler
        //Scheduler scheduler = new Scheduler();
        //scheduler.Start();

        while (true)
        {
            Thread.Sleep(60000);
        }
    }

    private static void ReadConsoleCommands()
    {
        Console.WriteLine("Type 'exit' to quit, or any " +
            "command to process it:");

        while (true)
        {
            string? command = Console.ReadLine();

            if (command == "exit")
            {
                Environment.Exit(0);
            }
                
            // Raise the command received event
            ReceivedCommandHandler?.Invoke(null, command ?? "");
        }
    }

    private static void OnReceivedCommand(object? sender, string command)
    {
        Console.WriteLine($"Command received: {command}");
            
        switch (command)
            {
                case "test task":
                    //Server.SendTestTask();
                    break;
                default:
                    Console.WriteLine("This command does not exist. Get some help.");
                    break;
            }
    }

    static void CatchUnhandledExceptions(object sender, 
        UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception) args.ExceptionObject;
        Console.WriteLine("MyHandler caught : " + e.Message);
        Console.WriteLine("Runtime terminating: {0}", 
            args.IsTerminating);
    }
}
