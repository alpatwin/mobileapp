using System.Windows;
using System.Windows.Controls;

namespace Toggl.WPF.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserControl mainView;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void SetMainView(UserControl mainView)
        {
            //this.mainView = null;
            if (this.mainView != null)
            {
                this.root.Children.Remove(this.mainView);
            }
            this.mainView = mainView;
            this.root.Children.Add(mainView);
        }
    }
}
