using SharedResources.Enums;

namespace SchedulerClientApp.ViewModels
{
    public class StatusParameters
    {
        public string ServerConnection { get; set; } = "";
        public ClientStatus ClientStatus { get; set; } = ClientStatus.Disconnected;
        public string TaskAssigned { get; set; } = "";
        public string TaskStatus { get; set; } = "";
        public string OperatingSystem { get; set; } = "";
        public string Cluster { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string ClientIP { get; set; } = "";
    }
}
