using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using JetBrains.Annotations;
using Nice3point.Revit.Extensions;
using Nice3point.Revit.Toolkit.External;
using Revit.Async;
using RevitAddIn.CommonUtils;
using RevitAddIn.ExtensionsUtils;
using RevitAddIn.RevitFailure;
using RevitAddIn.RevitSelectionFilter;
using RevitAddIn.ViewModels;
using RevitAddIn.Views;
using ricaun.Revit.UI.StatusBar;
using ricaun.Revit.UI.StatusBar.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace RevitAddIn.Commands;

/// <summary>
///     External command entry point invoked from the Revit interface
/// </summary>
[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class StartupCommand : ExternalCommand
{
    public override void Execute()
    {
        RevitTask.Initialize(UiApplication);

        Trace.TraceInformation("StartupCommand");
        var viewModel = new RevitAddInViewModel();
        var view = new RevitAddInView(viewModel);
        view.Show(UiApplication.MainWindowHandle);


        try
        {
            //var elemSelectionFilter = new ElementSelectionFilter((elem) =>
            //{
            //    return elem is FamilyInstance instance && instance.Symbol.FamilyName.Contains("ELE_EEQ");
            //});
            //var reference = UiDocument.Selection.PickObject(ObjectType.Element, elemSelectionFilter, "选择基准构件");

            //var baseFamilyInstance = Document.GetElement(reference) as FamilyInstance;

            //var otherReference = UiDocument.Selection.PickObject(ObjectType.Element, elemSelectionFilter, "选择需要连接的构件");

            //var needLinkFamilyInstance = Document.GetElement(otherReference) as FamilyInstance;

            //var baseElemConnectorList = GetConnectors(baseFamilyInstance);
            //var needLinkElemConnectorList = GetConnectors(needLinkFamilyInstance);


            //using Transaction trans = new(Document, "name");
            //trans.Start();

            //var notConnector1 = baseElemConnectorList.FirstOrDefault(a => !a.IsConnected);
            //var notConnector2 = needLinkElemConnectorList.FirstOrDefault(a => !a.IsConnected);

            //if (notConnector1 != null && notConnector2 != null)
            //{
            //    notConnector1.ConnectTo(notConnector2);
            //}

            //var location1 = (baseFamilyInstance.Location as LocationPoint).Point;
            //var location2 = (needLinkFamilyInstance.Location as LocationPoint).Point;

            //trans.Commit();


            //Transaction trans = new Transaction(Document, "name");
            //trans.Start();

            //var modelCurve1 = ModelCurveUtils.CreateModelLine(Document, line);
            //var modelCurve2 = ModelCurveUtils.CreateModelLine(Document, newLine);


            //trans.Commit();

            //Trace.TraceInformation("instances Count =" + instances.Count);



            //var elementIds = UiDocument.Selection.GetElementIds();

            //if (elementIds.Count == 0)
            //{
            //    TaskDialog.Show("Revit", "Select elements to copy.");
            //    return;
            //}

            //using (var revitProgressBar = new RevitProgressBar(true))
            //{
            //    using (Transaction transaction = new Transaction(Document))
            //    {
            //        transaction.Start("Copy Elements");

            //        revitProgressBar.Run("Copy Elements", 50, (i) =>
            //        {
            //            ElementTransformUtils.CopyElements(Document, elementIds, XYZ.BasisY * (i + 1));
            //        });

            //        if (revitProgressBar.IsCancelling())
            //            transaction.RollBack();
            //        else
            //            transaction.Commit();
            //    }
            //}

            ////ShowBalloonTip("CopyElements", this.GetType().Name, "Test ShowBalloonTip");
            //ShowBalloonTip("Message1", $"{nameof(StartupCommand)}", "http://www.baidu.com");
            //BalloonUtils.Show("Message2", $"{nameof(StartupCommand)}", "http://www.baidu.com");

        }
        catch (Exception ex)
        {
            Trace.TraceInformation("ex = " + ex.StackTrace);
        }
    }

   
}
