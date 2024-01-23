namespace SchedulerServerApp.DBModule;

public class DBClientMachineModel
{
    public string? Client_name { get; set; }
    public string? IP { get; set; }
    public string? Status { get; set; }
    public string? Last_status_msg_time { get; set; }
    public string? Operating_system { get; set; }
    public string? Cluster { get; set; }
    public string? Graphics_card { get; set; }
    public string? Installed_software { get; set; }
}
