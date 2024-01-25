using SchedulerClientApp.ClientModule;
using SchedulerClientApp.Services;
using SharedResources.Enums;
using SharedResources.Messages;
using System;
using System.Diagnostics;

namespace SchedulerClientApp.TaskManager;

public class SchedulerTask : ISchedulerTask
{
    public int ID { get; set; } = 0;
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Group { get; set; } = 0;
    public SchedulerTaskStatus Status { get; set; }
    public int Priority { get; set; }
    public DateTime TimeCreated { get; set; }
    public string ExeFilePath { get; set; } = "";
    public string InputFilesPath { get; set; } = "";
    public string OutputFilesPath { get; set; } = "";
    public string OperatingSystem { get; set; } = "";
    public DateTime TimeCompleted { get; set; }
    public int UserID { get; set; } = 0;

    public LogService? LogService { get; set; }
    public SchedulerClient Client { get; set; }
    
    // Macro for simpler way of logging messages
    public delegate void MacroDelegate(string message, bool endl = true,
        bool date = true);
    private readonly MacroDelegate Log;

    public SchedulerTask(LogService logService, SchedulerClient client)
    {
        LogService = logService;
        Log = LogService.Log;
        Client = client;
    }

    public void ExecuteTask()
    {
        // TODO - try catch block
        Process process = new Process();
        process.StartInfo.FileName = ExeFilePath;
        Log(ExeFilePath);
        process.Start();

        //Process.Start(@"C:\Program Files\Mozilla Firefox\firefox.exe");

        while (!process.HasExited)
        {
            System.Threading.Thread.Sleep(1000); // Sleep for 1 second
        }

        int exitCode = process.ExitCode;
        Log("Process has exited. Exit code: " + exitCode);


        // Send confirmation message that task is done

        string task_id = ID.ToString();
        TaskFinishedMessage finished_mess = new TaskFinishedMessage(task_id);
        Client.SendMessage(finished_mess);
        Log("Sending Task Finished Message.");

        // Set tag task assigned to false - harakiri (delete itself)
        Client.DeleteCurrentTask();
    }
}
