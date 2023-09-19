using SharedResources.Enums;
using System;
using System.Diagnostics;

namespace SchedulerClientApp.TaskManager;

public class SchedulerTask : ISchedulerTask
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
    public string UserID { get; set; } = "";
    public string BucketId { get; set; } = "";
    public SchedulerTaskStatus Status { get; set; }
    public int Priority { get; set; }
    public DateTime TimeCreated { get; set; }
    public string ExeFilePath { get; set; } = "";
    public string InputFilesPath { get; set; } = "";
    public string OutputFilesPath { get; set; } = "";
    public string OperatingSystem { get; set; } = "";

    public void ExecuteTask()
    {
        Process.Start(ExeFilePath);
        //Process.Start(@"C:\Program Files\Mozilla Firefox\firefox.exe");
    }
}
