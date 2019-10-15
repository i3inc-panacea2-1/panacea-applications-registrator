using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using PanaceaLib;
using SocketIOClient;
using TerminalIdentification;

namespace PanaceaRegistrator
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _seconds;
        private Client _socket;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task InitializeWebsocket()
        {
            try
            {
                _socket = new Client(App.Server);
                var webcams = new List<string>();
                var audioout = new List<string>();
                Putik.Text = (await TerminalIdentificationManager.GetIdentificationInfoAsync()).Putik;
                _socket.Error += (oo, ee) =>
               {
                   Dispatcher.Invoke(async () =>
                   {
                       status.Text = "Check internet connection.";
                       await RestartApplicationWithCounter(_socket, 4);
                   });

               };
                _socket.On("connect", msg => Dispatcher.Invoke(async () =>
                {

                    _socket.Emit("register",
                        new
                        {
                            mac = (await TerminalIdentificationManager.GetIdentificationInfoAsync()).Putik,
                            data = new
                            {
                                mac_addresses = new[]
                                    {(await TerminalIdentificationManager.GetIdentificationInfoAsync()).Putik},
                                name = Environment.MachineName,
                                devices = new { videoin = webcams, audioin = audioout }
                            }
                        });

                    status.Text = "Connected! Registration was sent...";
                    await RestartApplicationWithCounter(_socket, 120);
                }));
                _socket.On("disconnect", msg => Dispatcher.Invoke(async () =>
                {
                    status.Text = "Connection lost";
                    await RestartApplicationWithCounter(_socket);
                }));
                _socket.On("registerDeclined", msg => Dispatcher.Invoke(async () =>
                {
                    status.Text = "Registration declined :(";
                    await RestartApplicationWithCounter(_socket);
                }));
                _socket.On("errMacLicensed", msg => Dispatcher.Invoke(async () =>
                {
                    status.Text = "Mac address already exists :/";
                    await RestartApplicationWithCounter(_socket);
                }));


                _socket.On("putikExists", msg => Dispatcher.Invoke(async () =>
                {
                    status.Text = "Terminal Id already exists :/";
                    await RestartApplicationWithCounter(_socket);
                }));




                _socket.On("registerPending",
                    msg => Dispatcher.Invoke(() => { status.Text = "Registration is pending..."; }));
                _socket.On("registerSuccessful", msg => Dispatcher.Invoke(async () =>
                {
                    status.Text = "Registration successful...";
                    await RestartApplicationWithCounter(_socket, 10);
                }));
                _socket.Message += (oo, ee) => Console.WriteLine(ee.Message.MessageText);

                _socket.Connect();
                Dispatcher.Invoke(() => { status.Text = "Connecting..."; });
            }
            catch (Exception ex)
            {
                await Dispatcher.Invoke(async () =>
                {
                    status.Text = ex.Message;
                    await RestartApplicationWithCounter(_socket, 10);
                });
            }
        }

        private async Task RestartApplicationWithCounter(Client socket, int s = 10)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.IsEnabled = false;
            }
            _seconds = s;
            await Task.Delay(3000);
            try
            {
                if (_timer == null)
                {
                    _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 1) };
                    _timer.Tick += (oo, ee) =>
                    {
                        status.Text = "Panacea will restart in " + _seconds;

                        if (_seconds <= 0)
                        {
                            _timer.Stop();
                            socket.Close();
                            socket.Dispose();
                            Dispatcher.Invoke(() =>
                            {
                                Process.Start(Utils.GetPath("..", "..", "Updater", "IBT.Updater.exe"));
                                Application.Current.Shutdown();
                            });
                        }
                        _seconds--;
                    };
                }
                _timer.IsEnabled = true;
                _timer.Start();
            }
            catch
            {
                _timer?.Stop();
            }
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Common.BringWindowInFront(this);
            await InitializeWebsocket();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

    public static class Utils
    {
        public static string GetPath(params string[] parts)
        {
            return Path.Combine(new string[] { Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) }.Concat(parts).ToArray());
        }
    }
}