using System.ComponentModel;
using System.ServiceProcess;

namespace logsService
{
    [RunInstaller(true)]
    public partial class InstallerLogsService : System.Configuration.Install.Installer
    {
        readonly ServiceInstaller _serviceInstaller;
        readonly ServiceProcessInstaller _processInstaller;

        public InstallerLogsService()
        {
            InitializeComponent();
            _serviceInstaller = new ServiceInstaller();
            _processInstaller = new ServiceProcessInstaller();

            _processInstaller.Account = ServiceAccount.LocalSystem;
            _serviceInstaller.StartType = ServiceStartMode.Automatic;
            _serviceInstaller.ServiceName = "LogsService";

            Installers.Add(_processInstaller);
            Installers.Add(_serviceInstaller);
        }
    }
}
