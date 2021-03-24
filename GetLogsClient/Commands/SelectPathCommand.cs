using System.Windows.Forms;
using GetLogsClient.ViewModels;

namespace GetLogsClient.Commands
{
    public class SelectPathCommand : CommandBase<SelectPathCommand>
    {
        public override void Execute(object parameter)
        {
            if (parameter is IMainWindowViewModel viewModel)
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        viewModel.SaveDirectory = dialog.SelectedPath;
                    }
                }
            }
        }
    }
}