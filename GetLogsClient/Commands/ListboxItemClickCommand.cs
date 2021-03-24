using GetLogsClient.ViewModels;

namespace GetLogsClient.Commands
{
    public class ListboxItemClickCommand : CommandBase<ListboxItemClickCommand>
    {
        public override void Execute(object parameter)
        {
            if (parameter is IMainWindowViewModel viewModel)
            {
                viewModel.ViewFileInExternalEditor();
            }
        }
    }
}