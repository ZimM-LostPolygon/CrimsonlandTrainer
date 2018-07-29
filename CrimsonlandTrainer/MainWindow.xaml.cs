using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrimsonlandTrainer.Game;
using Path = System.IO.Path;

// ReSharper disable LocalizableElement

namespace CrimsonlandTrainer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public unsafe partial class MainWindow : Window {
        private string ChannelName;

        public MainWindow() {
            InitializeComponent();
            PerkToAddComboBox.ItemsSource =
                PerkToAddComboBox.ItemsSource
                    .Cast<Perk>()
                    .OrderBy(perk => perk.GetDescription().Name);

            return;
            Hide();
            Process clProcess = null;
            while (clProcess == null) {
                clProcess =
                    Process.GetProcesses()
                        .FirstOrDefault(process => process.ProcessName == "Crimsonland" || process.ProcessName == "Crimsonland-D3D11");
                if (clProcess == null) {
                    Console.WriteLine("any key to get cl");
                    Console.ReadLine();
                }
            }

            TestTrainer2.Run(clProcess);

            return;

            //
            //byte[] pickUpWeaponSignature = HexUtility.HexStringToBytes("558BEC83E4F8803DA90FBF0F0056578BF28BF97523803D9A0EBF0F00751A56E89C94F9FF83FE0F750F83F8327C0AB9D4");


            /*RemoteHooking.IpcCreateServer<TrainerInterface>(ref ChannelName, WellKnownObjectMode.Singleton);
            string injectionLibrary = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CrimsonlandTrainer.Hook.dll");
            EasyHook.RemoteHooking.Inject(
                clProcess.Id,          // ID of process to inject into
                injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                ChannelName         // the parameters to pass into injected library
                // ...
            );*/
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
            ((TrainerViewModel) DataContext).ApplicationStartCommand.Execute();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e) {
            ((TrainerViewModel) DataContext).ApplicationCloseCommand.Execute();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e) {
            //throw new NotImplementedException();
        }

        private void lbxs_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            //throw new NotImplementedException();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox) sender).ScrollToEnd();
        }
    }
}
