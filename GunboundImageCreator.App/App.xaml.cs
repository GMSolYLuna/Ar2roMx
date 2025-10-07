using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace GunboundImageCreator.App
{
    using System;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private Thread _proccessThread;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = System.Windows.ShutdownMode.OnLastWindowClose;
            OnStartup(e);
            ShowSlpash();
        }

        private void CheckForProcess()
        {
            _proccessThread = new Thread(KillProcess);
            _proccessThread.Start();
            Exit += (s, a) =>
                        {
                            if (_proccessThread != null)
                                _proccessThread.Abort();
                        };
        }

        private static void ShowSlpash()
        {
            var splashScreen = new SplashScreen("AGICSplashScreen.png");
            splashScreen.Show(false, true);
            Thread.Sleep(1000);
            splashScreen.Close(TimeSpan.FromSeconds(1));
        }

        private void KillProcess()
        {
            string[] p = {"ollydbg", "reflector", "dotpeek"};
            while (true)
            {
                var processes = Process.GetProcesses();

                foreach (var pr in processes)
                {
                    var s = pr.ProcessName.ToLower();
                    if (p.Any(e => e == s))
                    {
                        pr.Kill();
                    }
                }

                Thread.Sleep(300);
            }
        }

        private Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            CheckLocalization();

            bool isnew;
            _mutex = new Mutex(true, "Ar2roMxGunboundImageCreator2012", out isnew);

            if (!isnew)
            {
                Current.Shutdown();
            }

            CheckForProcess();
        }

        private void CheckLocalization()
        {
            File.SetAttributes(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, FileAttributes.Normal);

            var x = ConfigurationManager.AppSettings["Culture"];

            if (x != null)
            {
                try
                {
                    GunboundImageCreator.App.Properties.Resources.Culture = new System.Globalization.CultureInfo(x);
                }
                catch
                {
                    GunboundImageCreator.App.Properties.Resources.Culture = new System.Globalization.CultureInfo("es-ES");
                }
            }
            else
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("Culture");
                config.AppSettings.Settings.Add("Culture", "es-ES");
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                GunboundImageCreator.App.Properties.Resources.Culture = new System.Globalization.CultureInfo("es-ES");
            }

            File.SetAttributes(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, FileAttributes.ReadOnly);
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
        }
    }
}
