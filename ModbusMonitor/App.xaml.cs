using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ModbusMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                string s = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                    + " " + e.Exception.Message + " " + e.Exception.StackTrace;
                e.Handled = true;
                MessageBox.Show(s);
            }
            catch { };
        }
    }
}
