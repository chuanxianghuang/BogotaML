using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Nice3point.Revit.Extensions;
using Nice3point.Revit.Toolkit.External;
using Revit.Async;
using RevitAddIn.CommonUtils;
using RevitAddIn.ExtensionsUtils;
using RevitAddIn.RevitSelectionFilter;
using RevitAddIn.ViewModels;
using RevitAddIn.Views;
using ricaun.Revit.UI.StatusBar.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class EEEConnectCommand : ExternalCommand
    {
        private static EEEConnectWindow window;
        private static EEEConnectViewModel viewModel;
       
        public override void Execute()
        {
           
            Trace.TraceInformation("EEEConnectCommand");

            if (window != null)
            {
                if (window.IsLoaded)
                {
                    window.Focus();
                }
                else
                {
                    ShowWindow();
                }
            }
            else
            {
                RevitTask.Initialize(UiApplication);
                ShowWindow();
            }
        }

        private void ShowWindow()
        {
            viewModel = new EEEConnectViewModel();
            window = new EEEConnectWindow()
            {
                DataContext = viewModel,
            };
            window.Show(UiApplication.MainWindowHandle);
        }

    }
}
