namespace SchedulerClientApp.Services;

public interface ILogService
{
    void Log(string message, bool endl = true, bool date = true);
}
