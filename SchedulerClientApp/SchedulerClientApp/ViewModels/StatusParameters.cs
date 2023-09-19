using SharedResources.Enums;

namespace SchedulerClientApp.ViewModels
{
    public class StatusParameters
    {
        public string ServerConnection { get; set; } = "";
        public ClientStatus ClientStatus { get; set; } = ClientStatus.Disconnected;
        public string TaskAssigned { get; set; } = "";
        public SchedulerTaskStatus TaskStatus { get; set; } = 
            SchedulerTaskStatus.NoAssignedTask;
        public string OperatingSystem { get; set; } = "";
        public string Cluster { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string ClientIP { get; set; } = "";
    }
}
