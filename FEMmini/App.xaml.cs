using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FEMmini
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var windowVM = new MainWindowVM();

            var mainWindow = new MainWindow(windowVM)
            {
                //DataContext = windowVM,
            };
            mainWindow.Show();
        }
    }
}
