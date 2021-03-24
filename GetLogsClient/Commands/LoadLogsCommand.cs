using GetLogsClient.ViewModels;

namespace GetLogsClient.Commands
{
    public class LoadLogsCommand : CommandBase<LoadLogsCommand>
    {
        public override void Execute(object parameter)
        {
            if (parameter is IMainWindowViewModel viewModel)
            {
                viewModel.DownloadArchive();
            }
        }
    }
}