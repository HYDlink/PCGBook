using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PCG.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void Wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
            Application.Current.Dispatcher.Invoke(
                () => { },
                System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
    }
}