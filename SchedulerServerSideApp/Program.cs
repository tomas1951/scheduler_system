namespace SchedulerServerSideApp
{
    internal class Program
    {
        private static event EventHandler<string>? OnCommandReceived;
        const int PORT_NO = 1234;
        private static Server? Server;

        static void Main(string[] args)
        {
            // Exception catcher handler
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += 
                new UnhandledExceptionEventHandler(CatchUnhandledExceptions);
            
            // Console commands
            OnCommandReceived += HandleCommandReceived;
            Thread commandsThead = new Thread(ReadConsoleCommands);
            commandsThead.Start();

            // Start the server
            Server = new Server(PORT_NO);
            Server.Start();

            // Start the scheduler
            //Scheduler scheduler = new Scheduler();
            //scheduler.Start();

            while (true)
            {
                Thread.Sleep(60000);
                Console.WriteLine("App running.");
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
                    // Exit the application
                    Environment.Exit(0);
                }

                // Raise the command received event
                OnCommandReceived?.Invoke(null, command);
            }
        }

        private static void HandleCommandReceived(object? sender, string command)
        {
            Console.WriteLine($"Command received: {command}");
        }

        static void CatchUnhandledExceptions(object sender, 
            UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception) args.ExceptionObject;
            System.Console.WriteLine("MyHandler caught : " + e.Message);
            System.Console.WriteLine("Runtime terminating: {0}", 
                args.IsTerminating);
        }
    }
}
