using SchedulerClientApp.ViewModels;

namespace SchedulerClientApp.Services;

public class LogService : ILogService
{
    MainViewModel mainViewModel;

    public LogService(MainViewModel mainViewModel)
    {
        this.mainViewModel = mainViewModel;
    }

    public void Log(string message, bool endl = true, bool date = true)
    {
        if (endl)
        {
            message += "\n";
        }
        if (date)
        {
            System.DateTime currentDateTime = System.DateTime.Now;
            string formattedDate = currentDateTime.ToString("MM/dd/yyyy HH:mm:ss");
            message = formattedDate + " > " + message;
        }
        mainViewModel.ReceivedMessages += message;

    }
}
