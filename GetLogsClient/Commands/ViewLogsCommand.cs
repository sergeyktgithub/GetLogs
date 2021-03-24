using GetLogsClient.ViewModels;

namespace GetLogsClient.Commands
{
    public class ViewLogsCommand : CommandBase<ViewLogsCommand>
    {
        public override void Execute(object parameter)
        {
            if (parameter is IMainWindowViewModel viewModel)
            {
                viewModel.GiveGlanceLogFiles();
            }
        }
    }
}