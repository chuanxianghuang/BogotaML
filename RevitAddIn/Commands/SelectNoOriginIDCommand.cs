using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.Commands
{
    /// <summary>
    /// 命令名：SelectNoOriginID，鼠标提示：选择OriginID参数值为空的构件。可能是插件合模过程中产生，也可能是合模后调整过程中产生。
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class SelectNoOriginIDCommand : IExternalCommand
    {
        public Result Execute(Autodesk.Revit.UI.ExternalCommandData CommandData, ref string Message, ElementSet elementset)
        {
            Autodesk.Revit.ApplicationServices.Application Revit = CommandData.Application.Application;
            Autodesk.Revit.UI.UIDocument uidoc = CommandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = CommandData.Application.ActiveUIDocument.Document;
            ICollection<ElementId> currentselectIdList = uidoc.Selection.GetElementIds();
            Autodesk.Revit.DB.View curview = doc.ActiveView;

            #region 过滤类别
            Categories cgs = doc.Settings.Categories;

            ElementId pfCGID = cgs.get_Item(BuiltInCategory.OST_PipeFitting).Id;
            ElementId paCGID = cgs.get_Item(BuiltInCategory.OST_PipeAccessory).Id;
            ElementId springklerCGID = cgs.get_Item(BuiltInCategory.OST_Sprinklers).Id;
            ElementId lightFixtureCGID = cgs.get_Item(BuiltInCategory.OST_LightingFixtures).Id;
            ElementId lightDevicesCGID = cgs.get_Item(BuiltInCategory.OST_LightingDevices).Id;

            Dictionary<ElementId, string> categoryNameDic = new Dictionary<ElementId, string>();
            foreach (Category category in cgs)
            {
                categoryNameDic.Add(category.Id, category.Name);
            }

            List<BuiltInCategory> categoryExceptList = new List<BuiltInCategory>()
                    {
                    //相机
                    BuiltInCategory.OST_Cameras,
                    //参照平面
                    BuiltInCategory.OST_CLines,
                    //线
                    BuiltInCategory.OST_Lines,
                     //楼层
             BuiltInCategory.OST_Levels, 
                    //楼梯栏杆
                    BuiltInCategory.OST_StairsRailing,
                    //楼梯顶扶栏
                    BuiltInCategory.OST_RailingTopRail,
                    //楼梯平台
                    BuiltInCategory.OST_StairsLandings,
                    //楼梯梯段
                    BuiltInCategory.OST_StairsRuns,
                    //楼梯支撑
                    BuiltInCategory.OST_StairsStringerCarriage,
                    //楼板洞口
                    BuiltInCategory.OST_FloorOpening,
                    //幕墙竖挺
                    BuiltInCategory.OST_CurtainWallMullions,
                    //幕墙嵌板
                    BuiltInCategory.OST_CurtainWallPanels,
                     //门窗
                    BuiltInCategory.OST_Doors,
                    BuiltInCategory.OST_Windows,
                    //墙饰条
                    BuiltInCategory.OST_Cornices,
                     //分格缝
                    BuiltInCategory.OST_Reveals,
                    //中心线
                    BuiltInCategory.OST_PipeCurvesCenterLine,
                    BuiltInCategory.OST_PipeFittingCenterLine,
                    BuiltInCategory.OST_CableTrayCenterLine,
                    BuiltInCategory.OST_CableTrayFittingCenterLine,
                    BuiltInCategory.OST_DuctCurvesCenterLine,
                    BuiltInCategory.OST_DuctFittingCenterLine,
                    BuiltInCategory.OST_ConduitFittingCenterLine,
                    BuiltInCategory.OST_ConduitFittingCenterLine,
                    //线管管路
                    BuiltInCategory.OST_ConduitRun,
                    //高程点
                    BuiltInCategory.OST_SpotElevations,
                    //范围框
                    BuiltInCategory.OST_VolumeOfInterest,                    
                    //剖面框
                    BuiltInCategory.OST_SectionBox,
                    //系统
                    BuiltInCategory.OST_PipingSystem,
                    BuiltInCategory.OST_DuctSystem,
                    //明细表
                    BuiltInCategory.OST_Schedules,
                    //图纸
                    BuiltInCategory.OST_Sheets,
                    //填充区域
                    BuiltInCategory.OST_DetailComponents,
                    //空间
                    BuiltInCategory.OST_MEPSpaces,
                    //导向轴网
                    BuiltInCategory.OST_GuideGrid
                    };

            ICollection<ElementId> categoryIdExceptList = new List<ElementId>();

            foreach (BuiltInCategory bic in categoryExceptList)
            {
                try
                {
                    categoryIdExceptList.Add(cgs.get_Item(bic).Id);
                }
                catch
                { }
            }
            //Camera很奇怪，写死
            categoryIdExceptList.Add(new ElementId(-2000500));
            #endregion

            ElementMulticategoryFilter mcf = new ElementMulticategoryFilter(categoryIdExceptList, true);
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<Element> elementsWithOriginFile = collector
            .WhereElementIsNotElementType()
            .WherePasses(mcf)
            .Where(e => e.LookupParameter("OriginFile") != null)
            .Where(e => string.IsNullOrWhiteSpace(e.LookupParameter("OriginFile").AsString())) // 过滤 OriginID 值
            .ToList();

            uidoc.Selection.SetElementIds(elementsWithOriginFile.Select(e => e.Id).ToList());
            MessageBox.Show("已选择没有OriginFile参数或参数值为空的构件共 " + elementsWithOriginFile.Count + " 个。", "向日葵", MessageBoxButtons.OK);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
