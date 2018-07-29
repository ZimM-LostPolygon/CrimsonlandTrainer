using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace CrimsonlandTrainer.Utility
{
    /// <summary>
    /// Utility for showing specific TaskDialogs.
    /// </summary>
    public class TaskDialogUtility {
        public static TaskDialog CreateErrorTaskDialog(
            string windowTitle,
            string mainInstruction,
            string content,
            string errorText
        ) {
            TaskDialog taskDialog = new TaskDialog {
                WindowTitle = windowTitle,
                MainInstruction = mainInstruction,
                MainIcon = TaskDialogIcon.Error,
                Content = content,
                ExpandedInformation = errorText,
                FooterIcon = TaskDialogIcon.Information,
                Footer = @"<a href="""">" + "Copy Error Details" + "</a>",
                EnableHyperlinks = true
            };
            taskDialog.HyperlinkClicked += (sender, args) => Clipboard.SetText(errorText);
            taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));

            return taskDialog;
        }
    }
}
