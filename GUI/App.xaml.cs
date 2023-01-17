using System.Windows;
using Common;
using GUI.ViewModels;
using Logic;

namespace GUI
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var changeTracker = new ChangeTracker();
            var viewModel = new ViewModel(changeTracker);
            var mainWindow = new MainWindow(changeTracker)
            {
                DataContext = viewModel
            };

            mainWindow.Show();
        }
    }
}