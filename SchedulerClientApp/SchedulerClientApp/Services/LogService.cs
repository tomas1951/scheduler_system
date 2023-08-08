using SchedulerClientApp.ViewModels;

namespace SchedulerClientApp.Services
{
    public class LogService
    {
        MainViewModel mainViewModel;

        public LogService(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }

        public void Log(string message)
        {
            mainViewModel.ReceivedMessages += (message + "\n");
        }
    }
}
