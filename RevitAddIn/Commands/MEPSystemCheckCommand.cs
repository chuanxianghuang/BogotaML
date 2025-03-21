using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Windows;
using Nice3point.Revit.Toolkit.External;
using RevitAddIn.ExtensionsUtils;
using RevitAddIn.RevitFailure;
using RevitAddIn.RevitSelectionFilter;
using RevitAddIn.UICommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace RevitAddIn.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class MEPSystemCheckCommand : ExternalCommand
    {
        public override void Execute()
        {
            Trace.TraceInformation("MEPSystemCheckCommand");

            var elemSelectionFilter = new ElementSelectionFilter((elem) =>
            {
                var instance = elem as FamilyInstance;
                var condition = instance?.MEPModel?.ConnectorManager != null;
                if (condition)
                {
                    var familyName = instance.Symbol.FamilyName;
                    var instanceCategoryId = instance.Category.Id;
                    var targetCategoryId = Category.GetCategory(Document, BuiltInCategory.OST_ElectricalEquipment).Id;
                    condition = instanceCategoryId == targetCategoryId && familyName.Contains("ELE_EEQ") /*&& familyName.Contains("BASED")*/;
                }

                return condition;
            });
            var reference = UiDocument.Selection.PickObject(ObjectType.Element, elemSelectionFilter, "Select ELE_EEQ Base Element");

            var baseFamilyInstance = Document.GetElement(reference) as FamilyInstance;
            var mechanicalSystems = GetMechanicalSystems(baseFamilyInstance);
            if (mechanicalSystems.Count > 0)
            {
                MechanicalSystem mechanicalSystem = mechanicalSystems.FirstOrDefault();

                var elementSet = mechanicalSystem.DuctNetwork;
                List<ElementId> ids = new List<ElementId>();
                foreach (Element element in elementSet)
                {
                    if (element is FamilyInstance instance)
                    {
                        var connectorList = instance.GetConnectors();
                        foreach (var connector in connectorList)
                        {
                            if (connector.IsConnected)
                            {
                                var allRefConnectors = connector.AllRefs.ToList();
                                foreach (var allRefConnector in allRefConnectors)
                                {
                                    if (allRefConnector.Owner is not MEPSystem)
                                    {
                                        if (allRefConnector.Origin.DistanceTo(connector.Origin) > 0.01)
                                        {
                                            ids.Add(connector.Owner.Id);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (connector.Owner is not MEPSystem)
                                {
                                    ids.Add(connector.Owner.Id);
                                }

                            }
                        }
                    }
                } 

                if (ids.Count > 0)
                {

                    var failuresId = FailuresPreprocessor.CheckMEPSystemId;
                    FailuresPreprocessor.ProcessorFailuresMessage(Document, failuresId, ids);
                    
                    UiDocument.Selection.SetElementIds(ids);
                    //KeysPress.SetESC();
                }
            }
            else
            {
                TaskDialog.Show("Warning", "No Mechanical System found in the selected element");
            }
        }

        private List<MechanicalSystem> GetMechanicalSystems(FamilyInstance familyInstance)
        {
            var mechanicalSystems = new List<MechanicalSystem>();
            var connectorSet = familyInstance?.MEPModel?.ConnectorManager?.Connectors;
            if (connectorSet != null)
            {
                foreach (Connector connector in connectorSet)
                {
                    try
                    {
                        var mechanicalSystem = connector.MEPSystem as MechanicalSystem;
                        var isContained = mechanicalSystems.Any(a => a.Id == mechanicalSystem?.Id);
                        if (mechanicalSystem == null || isContained)
                        {
                            continue;
                        }
                        mechanicalSystems.Add(mechanicalSystem);
                    }
                    catch   { }
                }
            }

            return mechanicalSystems;
        }

       
    }
}
