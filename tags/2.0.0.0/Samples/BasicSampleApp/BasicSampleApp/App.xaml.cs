using System.Windows;

namespace BasicSampleApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var bootStrapper = new Bootstrapper();
            bootStrapper.Run();
        }
    }
}
