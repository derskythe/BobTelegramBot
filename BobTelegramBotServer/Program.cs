using System.ServiceProcess;

namespace BobTelegramBotServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] 
            { 
                new BobTelegramBotService() 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
