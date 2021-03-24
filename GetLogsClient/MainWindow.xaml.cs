using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Editors;
using GetLogsClient.ViewModels;

namespace GetLogsClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IMainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            NDateEditFrom.DateTime = DateTime.Now;
            NDateEditTo.DateTime = DateTime.Now;

            _viewModel = this.DataContext as IMainWindowViewModel;

            Loaded += OnLoaded;
            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            _viewModel.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel.ExternalInitializer();
        }

        private void DateEdit_EditValueChanged(object sender, DevExpress.Xpf.Editors.EditValueChangedEventArgs e)
        {
            if (NDateEditFrom.DateTime > NDateEditTo.DateTime)
            {
                NDateEditTo.DateTime = NDateEditFrom.DateTime;
            }
        }

        private void NInputPattern_OnProcessNewValue(DependencyObject sender, ProcessNewValueEventArgs e)
        {
            _viewModel.PatternList.Add(e.DisplayText);
        }
    }
}
