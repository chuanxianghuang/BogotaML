using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Nice3point.Revit.Toolkit.External;
using RevitAddIn.ExtensionsUtils;
using RevitAddIn.RevitSelectionFilter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class EEquipmentConnectCommand : ExternalCommand
    {
        public override void Execute()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-us");

                var elemSelectionFilter = new ElementSelectionFilter((elem) =>
                {
                    var instance = elem as FamilyInstance;
                    var condition = instance?.MEPModel?.ConnectorManager != null;
                    if (condition)
                    {
                        var familyName = instance.Symbol.FamilyName;
                        var instanceCategoryId = instance.Category.Id;
                        var targetCategoryId = Category.GetCategory(Document, BuiltInCategory.OST_ElectricalEquipment).Id;
                        condition = instanceCategoryId == targetCategoryId && familyName.Contains("ELE_EEQ") /* && familyName.Contains("BASED")*/;
                    }

                    return condition;
                });

                var reference1 = UiDocument.Selection.PickObject(ObjectType.Element,
                    elemSelectionFilter, "Select One Equipment Element");

                var reference2 = UiDocument.Selection.PickObject(ObjectType.Element,
                   elemSelectionFilter, "Select AnOther Equipment Element");

                var instance1 = Document.GetElement(reference1) as FamilyInstance;
                var instance2 = Document.GetElement(reference2) as FamilyInstance;

                var connectorList1 = instance1.GetConnectors();
                var connectorList2 = instance2.GetConnectors();

                Connector targetConnector1 = null;
                Connector targetConnector2 = null;
                double distance = double.MaxValue;
                foreach (var connector1 in connectorList1)
                {
                    foreach (var connector2 in connectorList2)
                    {
                        var distance1 = connector1.Origin.DistanceTo(connector2.Origin);
                        if (distance1.CompareTo(distance) <= 0)
                        {
                            distance = distance1;
                            targetConnector1 = connector1;
                            targetConnector2 = connector2;
                        }
                    }
                }

                if (targetConnector1 != null && targetConnector2 != null)
                {
                    string tip = string.Empty;
                    Transaction trans = new Transaction(Document);
                    if (targetConnector1.IsConnectedTo(targetConnector2))
                    {

                        tip = "Disconnected";
                    }
                    else
                    {

                        tip = "Connected";
                    }

                    if (distance > 0.01 && tip == "Connected")
                    {
                        TaskDialog taskDialog = new TaskDialog("Warning")
                        {
                            MainInstruction = "Warning Message:",
                            MainContent = $"The distance between two equipment is {Convert.ToInt32(distance.ToMillimeters())}（mm）, are you continue?",
                            CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel,  // 添加俩按钮
                            DefaultButton = TaskDialogResult.Cancel
                        };

                        var dialogResult = taskDialog.Show();
                        if (dialogResult == TaskDialogResult.Cancel)
                        {
                            return;
                        }
                    }

                    trans.SetName($"{tip} Electrical Equipment");
                    trans.Start();

                    if (tip == "Disconnected")
                    {
                        targetConnector1.DisconnectFrom(targetConnector2);
                    }
                    else
                    {
                        targetConnector1.ConnectTo(targetConnector2);
                    }

                    trans.Commit();

                    TaskDialog.Show("Electrical Equipment", tip);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Exception:" + ex.StackTrace);
            }
        }
    }
}
