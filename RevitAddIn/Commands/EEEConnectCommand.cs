using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using Nice3point.Revit.Extensions;
using Nice3point.Revit.Toolkit.External;
using RevitAddIn.CommonUtils;
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
    public class EEEConnectCommand : ExternalCommand
    {
        private readonly string[] elemParameterNames = { "LONGITUD X", "LONGITUD Y", "LONGITUD 1", "LONGITUD 2" };
        public override void Execute()
        {
            Trace.TraceInformation("EEEConnectCommand");
            try
            {
                var elemSelectionFilter = new ElementSelectionFilter((elem) =>
                   {

                       var instance = elem as FamilyInstance;
                       var condition = instance?.MEPModel?.ConnectorManager != null;

                       foreach (var parameterName in elemParameterNames)
                       {
                           condition = elem.ParametersMap.Contains(parameterName);
                           if (condition)
                           {
                               break;
                           }
                       }
                       if (condition)
                       {
                           var familyName = instance.Symbol.FamilyName;
                           var instanceCategoryId = instance.Category.Id;
                           var targetCategoryId = Category.GetCategory(Document, BuiltInCategory.OST_ElectricalEquipment).Id;
                           condition = instanceCategoryId == targetCategoryId && familyName.Contains("ELE_EEQ") /* && familyName.Contains("BASED")*/;
                       }

                       return condition;
                   });
                var reference1 = UiDocument.Selection.PickObject(ObjectType.Element, elemSelectionFilter, "Select Element");
                var reference2 = UiDocument.Selection.PickObject(ObjectType.Element, elemSelectionFilter, "Select Another Element");

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
                    Transaction trans = new Transaction(Document, "name");
                    trans.Start();

                    var category1 = Get_EEQ_Categrory(connectorList1);
                    var category2 = Get_EEQ_Categrory(connectorList2);

                    List<FamilyInstance> newInstances = new List<FamilyInstance>();
                    if (category1 == ELE_EEQ_Categrory.HorizontalStraight)
                    {
                        string parameterName = "LONGITUD X";
                        newInstances = Test(targetConnector1, targetConnector2, instance1, distance, parameterName);
                    }
                    else if (category1 == ELE_EEQ_Categrory.VerticalStraight)
                    {
                        string parameterName = "LONGITUD Y";
                        newInstances = Test(targetConnector1, targetConnector2, instance1, distance, parameterName);
                    }
                    else if (category1 == ELE_EEQ_Categrory.HorizontalElbow)
                    {
                        switch (category2)
                        {
                            case ELE_EEQ_Categrory.HorizontalStraight:
                                {
                                    string parameterName = "LONGITUD X";
                                    newInstances = Test(targetConnector2, targetConnector1, instance2, distance, parameterName);
                                }
                                break;
                            case ELE_EEQ_Categrory.VerticalStraight:
                                {
                                    string parameterName = "LONGITUD Y";
                                    newInstances = Test(targetConnector2, targetConnector1, instance2, distance, parameterName);
                                }
                                break;
                            case ELE_EEQ_Categrory.HorizontalElbow:
                            case ELE_EEQ_Categrory.VerticalElbox:
                                {
                                    //var familyName = instance1.Symbol.FamilyName;
                                    //var index = familyName.IndexOf("-");
                                    //var prefix = familyName.Substring(0, index);

                                    //var newFamilyName = $"{prefix}-A-HORIZONTAL-STRAIGHT-SECTION-LH";
                                    //var newFamily = new FilteredElementCollector(Document)
                                    //    .OfClass(typeof(Family))
                                    //    .Cast<Family>()
                                    //    .Where(x => x.Name == newFamilyName)
                                    //    .FirstOrDefault();
                                    //if (newFamily != null)
                                    //{
                                    //    var familySymbol = FamilyUitls.GetFamilySymbol(newFamily, newFamilyName);
                                    //    string parameterName = "LONGITUD X";
                                    //    newInstances = Test1(targetConnector1, targetConnector2, familySymbol, distance, parameterName);
                                    //}

                                    targetConnector1.ConnectTo(targetConnector2);
                                }
                                break;
                            case ELE_EEQ_Categrory.Unknown:
                            default:
                                break;
                        }
                    }
                    else if (category1 == ELE_EEQ_Categrory.VerticalElbox)
                    {
                        switch (category2)
                        {
                            case ELE_EEQ_Categrory.HorizontalStraight:
                                {
                                    string parameterName = "LONGITUD X";
                                    newInstances = Test(targetConnector2, targetConnector1, instance2, distance, parameterName);
                                }
                                break;
                            case ELE_EEQ_Categrory.VerticalStraight:
                                {
                                    string parameterName = "LONGITUD Y";
                                    newInstances = Test(targetConnector2, targetConnector1, instance2, distance, parameterName);
                                }
                                break;
                            case ELE_EEQ_Categrory.HorizontalElbow:
                                {
                                    //var familyName = instance1.Symbol.FamilyName;
                                    //var index = familyName.IndexOf("-");
                                    //var prefix = familyName.Substring(0, index);

                                    //var newFamilyName = $"{prefix}-A-HORIZONTAL-STRAIGHT-SECTION-LH";
                                    //var targetInstance = new FilteredElementCollector(Document)
                                    //    .OfClass(typeof(FamilyInstance))
                                    //    .Cast<FamilyInstance>()
                                    //    .Where(x => x.Name == newFamilyName)
                                    //    .FirstOrDefault();
                                    //if (targetInstance != null)
                                    //{

                                    //}

                                    targetConnector1.ConnectTo(targetConnector2);
                                }
                                break;
                            case ELE_EEQ_Categrory.VerticalElbox:
                                {
                                    //var familyName = instance1.Symbol.FamilyName;
                                    //var index = familyName.IndexOf("-");
                                    //var prefix = familyName.Substring(0, index);
                                    //Trace.TraceInformation("prefix=" + prefix);

                                    //string newFamilyName = $"{prefix}-A-VERTICAL-STRAIGHT-SECTION";
                                    //var targetInstance = new FilteredElementCollector(Document)
                                    //    .OfClass(typeof(FamilyInstance))
                                    //    .Cast<FamilyInstance>()
                                    //    .Where(x => x.Name == newFamilyName)
                                    //    .FirstOrDefault();
                                    //if (targetInstance != null)
                                    //{

                                    //}

                                    targetConnector1.ConnectTo(targetConnector2);
                                }
                                break;
                            case ELE_EEQ_Categrory.Unknown:
                            default:
                                break;
                        }
                    }
                    Document.Regenerate();

                    List<Connector> newConnectors = new List<Connector>();
                    foreach (var newInstance in newInstances)
                    {
                        newConnectors.AddRange(newInstance.GetConnectors());
                    }
                    newConnectors.Sort((x, y) => x.Origin.DistanceTo(targetConnector1.Origin).CompareTo(y.Origin.DistanceTo(targetConnector1.Origin)));
                    for (int i = 0; i < newConnectors.Count; i++)
                    {
                        if (i == 0)
                        {
                            newConnectors[i].ConnectTo(targetConnector1);
                        }

                        else if (i == newConnectors.Count - 1)
                        {
                            newConnectors[i].ConnectTo(targetConnector2);
                        }
                        else
                        {
                            newConnectors[i].ConnectTo(newConnectors[i + 1]);
                            i++;
                        }
                    }

                    //ModelCurveUtils.CreateModelArc(Document, instance1.GetLocation(), 200);
                    //ModelCurveUtils.CreateModelArc(Document, instance2.GetLocation(), 300);

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("EEEConnectCommand EX= " + ex.StackTrace);
            }
        }

        private List<FamilyInstance> Test(Connector targetConnector1, Connector targetConnector2, FamilyInstance instance, double distance,
            string parameterName)
        {
            List<FamilyInstance> newInstances = new List<FamilyInstance>();
            var dire = targetConnector1.CoordinateSystem.BasisZ;
            int ruleValue = 2000;
            int currentDistance = Convert.ToInt32(distance.ToMillimeters());
            int count = Convert.ToInt32(currentDistance / ruleValue);
            int temp = currentDistance % ruleValue;
            if (count > 0)
            {
                List<double> putLengths = new List<double>();
                for (int i = 0; i < count; i++)
                {
                    putLengths.Add(ruleValue);
                }

                if (temp < 200 && temp > 0)
                {
                    putLengths.Remove(0);
                    double half = (ruleValue + temp) / 2.0;
                    putLengths.Add(half);
                    putLengths.Add(half);
                }
                else if (temp >= 200)
                {
                    putLengths.Add(temp);
                }

                var origin = targetConnector1.Origin;
                var instance1_Location = instance.GetLocation();
                foreach (var putLength in putLengths)
                {
                    var copyElementIds = instance.Copy(XYZ.Zero);
                    if (copyElementIds.Count > 0)
                    {
                        var newFamilyInstance = Document.GetElement(copyElementIds.First()) as FamilyInstance;
                        var dispaly = putLength / 1000;
                        newFamilyInstance.FindParameter(parameterName).SetDisplayValueToParameter(dispaly);

                        if (instance1_Location.DistanceTo(targetConnector1.Origin) < 0.01)
                        {
                            origin += dire * putLength / 304.8;
                            (newFamilyInstance.Location as LocationPoint).Point = origin;
                        }
                        else
                        {
                            (newFamilyInstance.Location as LocationPoint).Point = origin;
                            origin += dire * putLength / 304.8;
                        }

                        newInstances.Add(newFamilyInstance);
                    }
                }
            }
            else
            {
                var instance1_Location = instance.GetLocation();
                if (temp < 200)
                {
                    targetConnector1.ConnectTo(targetConnector2);
                    var param_LONGITUDX = instance.FindParameter("parameterName");
                    var paramValue = param_LONGITUDX.GetFromInternalValue();
                    if (instance1_Location.DistanceTo(targetConnector1.Origin) < 0.01)
                    {

                        param_LONGITUDX.SetDisplayValueToParameter((paramValue + temp / 1000.0));
                        (instance.Location as LocationPoint).Point = instance1_Location + dire * temp / 304.8;
                    }
                    else
                    {
                        param_LONGITUDX.SetDisplayValueToParameter((paramValue + temp / 1000.0));
                    }

                }
                else
                {
                    var origin = targetConnector1.Origin;

                    var putLength = temp;
                    var copyElementIds = instance.Copy(XYZ.Zero);
                    if (copyElementIds.Count > 0)
                    {
                        var newFamilyInstance = Document.GetElement(copyElementIds.First()) as FamilyInstance;
                        var dispaly = putLength / 1000.0;
                        newFamilyInstance.FindParameter(parameterName).SetDisplayValueToParameter(dispaly);

                        if (instance1_Location.DistanceTo(targetConnector1.Origin) < 0.01)
                        {
                            origin += dire * putLength / 304.8;
                            (newFamilyInstance.Location as LocationPoint).Point = origin;
                        }
                        else
                        {
                            (newFamilyInstance.Location as LocationPoint).Point = origin;
                        }

                        newInstances.Add(newFamilyInstance);
                    }
                }
            }

            return newInstances;
        }

        private List<FamilyInstance> Test1(Connector targetConnector1, Connector targetConnector2, FamilySymbol familySymbol, double distance,
           string parameterName)
        {
            List<FamilyInstance> newInstances = new List<FamilyInstance>();
            var dire = targetConnector1.CoordinateSystem.BasisZ;
            int ruleValue = 2000;
            int currentDistance = Convert.ToInt32(distance.ToMillimeters());
            int count = Convert.ToInt32(currentDistance / ruleValue);
            int temp = currentDistance % ruleValue;
            if (count > 0)
            {
                List<double> putLengths = new List<double>();
                for (int i = 0; i < count; i++)
                {
                    putLengths.Add(ruleValue);
                }

                if (temp < 200 && temp > 0)
                {
                    putLengths.Remove(0);
                    double half = (ruleValue + temp) / 2.0;
                    putLengths.Add(half);
                    putLengths.Add(half);
                }
                else if (temp >= 200)
                {
                    putLengths.Add(temp);
                }

                var origin = targetConnector1.Origin;
                foreach (var putLength in putLengths)
                {
                    
                    var newFamilyInstance = Document.Create.NewFamilyInstance(origin, familySymbol, dire, null, StructuralType.NonStructural);
                    var dispaly = putLength / 1000;
                    newFamilyInstance.FindParameter(parameterName).SetDisplayValueToParameter(dispaly);

                    (newFamilyInstance.Location as LocationPoint).Point = origin;
                    origin += dire * putLength / 304.8;
                    newInstances.Add(newFamilyInstance);
                }
            }
            else
            {
                if (temp < 200)
                {
                    targetConnector1.ConnectTo(targetConnector2);
                    //var param_LONGITUDX = instance.FindParameter("parameterName");
                    //var paramValue = param_LONGITUDX.GetFromInternalValue();
                    //if (instance1_Location.DistanceTo(targetConnector1.Origin) < 0.01)
                    //{

                    //    param_LONGITUDX.SetDisplayValueToParameter((paramValue + temp / 1000.0));
                    //    (instance.Location as LocationPoint).Point = instance1_Location + dire * temp / 304.8;
                    //}
                    //else
                    //{
                    //    param_LONGITUDX.SetDisplayValueToParameter((paramValue + temp / 1000.0));
                    //}
                }
                else
                {
                    var origin = targetConnector1.Origin;

                    var putLength = temp;
                    var newFamilyInstance = Document.Create.NewFamilyInstance(origin, familySymbol, dire, null, StructuralType.NonStructural);
                    var dispaly = putLength / 1000.0;
                    newFamilyInstance.FindParameter(parameterName).SetDisplayValueToParameter(dispaly);

                    (newFamilyInstance.Location as LocationPoint).Point = origin;

                    newInstances.Add(newFamilyInstance);
                }
            }

            return newInstances;
        }


        private ELE_EEQ_Categrory Get_EEQ_Categrory(List<Connector> connectors)
        {
            var connector1 = connectors.FirstOrDefault();
            var connector2 = connectors.LastOrDefault();

            var conDire1 = connector1.CoordinateSystem.BasisZ;
            var conDire2 = connector2.CoordinateSystem.BasisZ;

            var connOrin1 = connector1.Origin;
            var connOrin2 = connector2.Origin;

            var condition1 = conDire1.IsAlmostEqualTo(-conDire2);
            var condition2 = Math.Abs(connOrin1.Z - connOrin2.Z) < 0.01;

            if (condition1)
            {
                return condition2 ? ELE_EEQ_Categrory.HorizontalStraight
                   : ELE_EEQ_Categrory.VerticalStraight;
            }
            else
            {
                return condition2 ? ELE_EEQ_Categrory.HorizontalElbow
                    : ELE_EEQ_Categrory.VerticalElbox;
            }
        }

    }

    public enum ELE_EEQ_Categrory
    {
        HorizontalStraight,
        VerticalStraight,
        HorizontalElbow,
        VerticalElbox,
        Unknown
    }
}
