using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using Revit.Async;
using RevitAddIn.Commands;
using RevitAddIn.ExtensionsUtils;
using RevitAddIn.RevitSelectionFilter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Electrical;
using RevitAddIn.UICommon;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;
using ricaun.Revit.UI.StatusBar.Utils;

namespace RevitAddIn.ViewModels
{
    public partial class EEEConnectViewModel : ObservableValidator
    {
        [ObservableProperty]
        private List<SelectedModel> selectedModels;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [CustomValidation(typeof(EEEConnectViewModel), nameof(ValidateRuleValue))]
        [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
        private string ruleValue;

        [ObservableProperty]
        private string tipMessage;

        [ObservableProperty]
        private bool isRuleValueEnable;

        private readonly string[] elemParameterNames =
        {
            "LONGITUD X",
            "LONGITUD Y",
            "LONGITUD 1",
            "LONGITUD 2"
        };

        public EEEConnectViewModel()
        {
            SelectedModels = new List<SelectedModel>()
            {
               new SelectedModel
               {
                    Content = "标准段",
                    IsChecked = true
               },

               new SelectedModel
               {
                    Content = "延伸",
                    IsChecked = false
               },
            };


            StringBuilder sb = new StringBuilder();
            sb.Append("选择两个电气设备构件：\n");
            sb.Append("1.都是竖向，或者横向的，以第一个为基准，延伸或者该构件的方向补缺；\n");
            sb.Append("2.都是弯头的，只会连接，可能导致连接的构件错位移动；\n");
            sb.Append("3.其中一个带有弯头的，以竖向，或者横向的为基准延伸或者该构件的方向补缺；\n");
            TipMessage = sb.ToString();

            RuleValue = "2000";
            IsRuleValueEnable = true;
        }

        #region RelayCommand

        [RelayCommand]
        private void Cancel()
        {
            KeysPress.SetESC();
        }

        [RelayCommand]
        public void SelectMode(SelectedModel selectedModel)
        {
            IsRuleValueEnable = selectedModel.Content == "标准段";
        }

        [RelayCommand(CanExecute = nameof(CanExecuteConfirm))]
        private async Task ConfirmAsync()
        {
            await RevitTask.RunAsync(app =>
            {
                var uiDoc = app.ActiveUIDocument;
                var doc = uiDoc.Document;

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
                        var targetCategoryId = Category.GetCategory(doc, BuiltInCategory.OST_ElectricalEquipment).Id;
                        condition = instanceCategoryId == targetCategoryId && familyName.Contains("ELE_EEQ") /* && familyName.Contains("BASED")*/;
                    }

                    return condition;
                });

                while (true)
                {
                    try
                    {
                        var reference1 = uiDoc.Selection.PickObject(ObjectType.Element, elemSelectionFilter, "Select Element");
                        var reference2 = uiDoc.Selection.PickObject(ObjectType.Element, elemSelectionFilter, "Select Another Element");

                        var instance1 = doc.GetElement(reference1) as FamilyInstance;
                        var instance2 = doc.GetElement(reference2) as FamilyInstance;

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
                            if (targetConnector1.IsConnected || targetConnector2.IsConnected)
                            {
                                TaskDialog.Show("Tip", "Equipment is already connected");
                                return;
                            }

                            Transaction trans = new Transaction(doc, "Electrical Equipment Connect and Strech");
                            trans.Start();

                            var category1 = Get_EEQ_Categrory(connectorList1);
                            var category2 = Get_EEQ_Categrory(connectorList2);

                            List<FamilyInstance> newInstances = new List<FamilyInstance>();
                            if (category1 == ELE_EEQ_Categrory.HorizontalStraight)
                            {
                                string parameterName = "LONGITUD X";
                                newInstances = Test(targetConnector1, targetConnector2, distance, parameterName);
                            }
                            else if (category1 == ELE_EEQ_Categrory.VerticalStraight)
                            {
                                string parameterName = "LONGITUD Y";
                                newInstances = Test(targetConnector1, targetConnector2, distance, parameterName);
                            }
                            else if (category1 == ELE_EEQ_Categrory.HorizontalElbow)
                            {
                                switch (category2)
                                {
                                    case ELE_EEQ_Categrory.HorizontalStraight:
                                        {
                                            string parameterName = "LONGITUD X";
                                            newInstances = Test(targetConnector2, targetConnector1, distance, parameterName);
                                        }
                                        break;
                                    case ELE_EEQ_Categrory.VerticalStraight:
                                        {
                                            string parameterName = "LONGITUD Y";
                                            newInstances = Test(targetConnector2, targetConnector1, distance, parameterName);
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
                                            newInstances = Test(targetConnector2, targetConnector1, distance, parameterName);
                                        }
                                        break;
                                    case ELE_EEQ_Categrory.VerticalStraight:
                                        {
                                            string parameterName = "LONGITUD Y";
                                            newInstances = Test(targetConnector2, targetConnector1, distance, parameterName);
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
                            doc.Regenerate();

                            List<Connector> newConnectors = new List<Connector>();
                            foreach (var newInstance in newInstances)
                            {
                                newConnectors.AddRange(newInstance.GetConnectors());
                            }
                            newConnectors.Sort((x, y) => x.Origin.DistanceTo(targetConnector1.Origin)
                            .CompareTo(y.Origin.DistanceTo(targetConnector1.Origin)));
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

                            BalloonUtils.Show("Message", "Completed");
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceInformation("EEEConnectCommand EX= " + ex.StackTrace);
                        break;
                    }
                }
            });
        }


        #endregion

        private bool CanExecuteConfirm()
        {
            return !HasErrors;
        }

        /// <summary>
        /// ValidateRuleValue
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ValidationResult ValidateRuleValue(string value)
        {
            int.TryParse(value, out int result);
            if (result >= 200 && result <= 3500)
            {
                return ValidationResult.Success;
            }
            return new("标准段的长度需>=200mm,<=3500mm");
        }

        private List<FamilyInstance> Test(Connector targetConnector1, Connector targetConnector2,
            double distance, string parameterName)
        {

            var instance = targetConnector1.Owner as FamilyInstance;
            var doc = instance.Document;

            var dire = targetConnector1.CoordinateSystem.BasisZ;
            int currentDistance = Convert.ToInt32(distance.ToMillimeters());
            List<FamilyInstance> newInstances = new List<FamilyInstance>();
            if (IsRuleValueEnable)
            {

                int ruleValue = int.Parse(RuleValue);

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
                            var newFamilyInstance = doc.GetElement(copyElementIds.First()) as FamilyInstance;
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
                        var tempDisplayValue = temp.MmToM();
                        targetConnector1.ConnectTo(targetConnector2);
                        var param_LONGITUDX = instance.FindParameter(parameterName);
                        var paramValue = param_LONGITUDX.GetFromInternalValue();
                        if (instance1_Location.DistanceTo(targetConnector1.Origin) < 0.01)
                        {
                            param_LONGITUDX.SetDisplayValueToParameter((paramValue + tempDisplayValue));
                            (instance.Location as LocationPoint).Point = instance1_Location + dire * temp.MmToFeet();
                        }
                        else
                        {
                            param_LONGITUDX.SetDisplayValueToParameter((paramValue + tempDisplayValue));
                        }
                    }
                    else
                    {
                        var origin = targetConnector1.Origin;

                        var putLength = temp;
                        var copyElementIds = instance.Copy(XYZ.Zero);
                        if (copyElementIds.Count > 0)
                        {
                            var newFamilyInstance = doc.GetElement(copyElementIds.First()) as FamilyInstance;
                            var dispaly = putLength.MmToM();
                            newFamilyInstance.FindParameter(parameterName).SetDisplayValueToParameter(dispaly);

                            if (instance1_Location.DistanceTo(origin) < 0.01)
                            {
                                origin += dire * putLength.FeetToMM();
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
            }
            else
            {
                var instance1_Location = instance.GetLocation();

                var tempDisplayValue = currentDistance.MmToM();
                targetConnector1.ConnectTo(targetConnector2);
                var param_LONGITUDX = instance.FindParameter(parameterName);
                var paramValue = param_LONGITUDX.GetFromInternalValue();
                if (instance1_Location.DistanceTo(targetConnector1.Origin) < 0.01)
                {
                    param_LONGITUDX.SetDisplayValueToParameter((paramValue + tempDisplayValue));
                    (instance.Location as LocationPoint).Point = instance1_Location + dire * distance;
                }
                else
                {
                    param_LONGITUDX.SetDisplayValueToParameter((paramValue + tempDisplayValue));
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

    public class SelectedModel : ObservableObject
    {
        public SelectedModel()
        {
        }

        private string content;
        public string Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        private bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set => SetProperty(ref isChecked, value);
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
