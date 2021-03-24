using System.ServiceProcess;

namespace logsService
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new LogsService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
