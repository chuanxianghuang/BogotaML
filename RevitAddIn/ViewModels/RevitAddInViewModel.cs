
using Autodesk.Revit.UI;
using Revit.Async;
using RevitAddIn.Commands;
using ricaun.Revit.UI.StatusBar;
using ricaun.Revit.UI.StatusBar.Utils;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace RevitAddIn.ViewModels
{
    public partial class RevitAddInViewModel : ObservableObject
    {
        [RelayCommand]
        private async Task TestAsync()
        {
            await RevitTask.RunAsync(app =>
            {
                var uiDoc = app.ActiveUIDocument;
                var doc = uiDoc.Document;
               
                var elementIds = uiDoc.Selection.GetElementIds();

                if (elementIds.Count == 0)
                {
                    TaskDialog.Show("Revit", "Select elements to copy.");
                    return;
                }
             
                using (var revitProgressBar = new RevitProgressBar(true))
                {
                    using Transaction transaction = new Transaction(doc);
                    transaction.Start("Copy Elements");

                    revitProgressBar.Run("Copy Elements", 50, (i) =>
                    {    
                        ElementTransformUtils.CopyElements(doc, elementIds, XYZ.BasisY * (i + 1));
                    });

                    
                    if (revitProgressBar.IsCancelling())
                    {
                        transaction.RollBack();
                    }
                    else
                    {
                        transaction.Commit();
                    }
                }

                BalloonUtils.Show("Copy Elements", $"{nameof(StartupCommand)}", "http://www.baidu.com");
            });
        }
    }
}