using System;
using System.IO;
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

//        private bool CreateLogFile()
//        {
//            try
//            {
//                string pathName = Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar + "SchedulerClientApp.log";
//#if (DEBUG)
//                Console.WriteLine("The current log file is {0}", pathName);
//#endif
//                if (!File.Exists(pathName))
//                {
//                    FileStream fs = File.Create(pathName);
//                }
//            }
//            catch (Exception e)
//            {
//                Log($"An error occurred: {e.Message}. Log file won't be saved in the file.");
//                return false;
//            }
//#if (DEBUG)
//            Console.WriteLine("Log file has been created successfully");
//#endif
//            return true;
//        }
    }
}
