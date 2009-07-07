using System.Windows;

namespace BasicSampleApp
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window, IShellView
    {
        public Shell()
        {
            InitializeComponent();
        }

        public void ShowView()
        {
            Show();
        }
    }
}
