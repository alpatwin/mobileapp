using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Toggl.Core.UI;
using Toggl.Core.UI.ViewModels;
using Toggl.WPF.Presentation;
using Toggl.WPF.Startup;

namespace Toggl.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            WpfDependencyContainer.EnsureInitialized();
            var app = new AppStart(WpfDependencyContainer.Instance);
            var viewModelLoader = WpfDependencyContainer.Instance.ViewModelLoader;
            var loginViewModel = viewModelLoader.Load<LoginViewModel>();
            var loginView = new LoginView(loginViewModel);
            var mainWindow = new MainWindow();
            mainWindow.Show();
            mainWindow.SetMainView(loginView);
        }
    }
}
