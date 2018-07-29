using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CrimsonlandTrainer.Utility;
using Ookii.Dialogs.Wpf;

namespace CrimsonlandTrainer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs eventArgs) {
            if (System.Diagnostics.Debugger.IsAttached)
                return;

            eventArgs.Handled = true;
            ShowUnhadledExceptionDialog(eventArgs.Exception);
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs eventArgs) {
            if (System.Diagnostics.Debugger.IsAttached)
                return;

            eventArgs.SetObserved();
            ShowUnhadledExceptionDialog(eventArgs.Exception);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs) {
            if (System.Diagnostics.Debugger.IsAttached)
                return;

            ShowUnhadledExceptionDialog((Exception) eventArgs.ExceptionObject);
        }

        private static void ShowUnhadledExceptionDialog(Exception e) {
            TaskDialog errorTaskDialog = TaskDialogUtility.CreateErrorTaskDialog(
                "Unhandled Exception",
                "Unhandled Exception",
                e.Message,
                e.ToString()
            );

            errorTaskDialog.ShowDialog();
            Current.Shutdown(3);
        }
    }
}
