using System.Windows;
using System.Windows.ApplicationExtensions;
using PanaceaLib;

namespace PanaceaRegistrator
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : SingleInstanceApp
    {
	    public App() : base("PanaceaRegistrator")
	    {
		    InitializeComponent();
	    }
        public static string Server { get; private set; }
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
	        if (e.Args.Length > 0)
	        {
		        Server = e.Args[0];
		        var w = new MainWindow();
				w.Show();
	        }

        }

		public override bool SignalExternalCommandLineArgs(System.Collections.Generic.IList<string> args)
		{
			return true;

		}
	}
}