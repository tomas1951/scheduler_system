﻿using SchedulerServerApp.ServerModule;

namespace SchedulerServerApp;

/// <summary>
/// This is the starting point of a Scheduler System Server side.
/// </summary>
internal class Program
{
    private static event EventHandler<string>? ReceivedCommandHandler;
    private const int PortNo = 1234;
    public static Server Server { get; set; } = new Server(PortNo);

    static void Main(string[] args)
    {
        // Catching unhandled exceptions
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException +=
            new UnhandledExceptionEventHandler(CatchUnhandledExceptions);

        // Handler for console commands
        ReceivedCommandHandler += OnReceivedCommand;
        Thread commandsThead = new Thread(ReadConsoleCommands);
        commandsThead.Start();

        // Start the scheduler
        //Scheduler scheduler = new Scheduler();
        //scheduler.Start();
    }

    private static void ReadConsoleCommands()
    {
        Console.WriteLine("Type 'exit' to quit, or any command to process it:");

        while (true)
        {
            string? command = Console.ReadLine();
            // Raise the command received event
            ReceivedCommandHandler?.Invoke(null, command ?? "");
        }
    }

    private static void OnReceivedCommand(object? sender, string command)
    {
        Console.WriteLine($"Command received: {command}");

        switch (command)
        {
            case "exit":
                Environment.Exit(0);
                break;
            case "test task":
                Server.TestSendTask();
                break;
            case "test disconnect":
                Server.TestDisconnect();
                break;
            default:
                Console.WriteLine("This command does not exist. Get some help.");
                break;
        }
    }

    static void CatchUnhandledExceptions(object sender, UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception)args.ExceptionObject;
        Console.WriteLine("MyHandler caught : " + e.Message);
        Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
    }
}
