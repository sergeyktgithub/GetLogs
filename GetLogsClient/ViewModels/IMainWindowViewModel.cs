using System;
using System.Collections.ObjectModel;

namespace GetLogsClient.ViewModels
{
    public interface IMainWindowViewModel : IDisposable
    {
        ObservableCollection<string> PatternList { get; set; }
        string SaveDirectory { get; set; }
     
        void ExternalInitializer();
        void GiveGlanceLogFiles();
        void ViewFileInExternalEditor();
        void DownloadArchive();
    }
}