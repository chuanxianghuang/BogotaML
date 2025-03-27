using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.Commands.SelectByOriginID
{
    /// <summary>
    /// 命令名：SelectByOriginID，鼠标提示：输入OriginID值，选择构件。用于查找原始模型构件在合模后的位置。
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class SelectByOriginIDCommand : IExternalCommand
    {
        public Result Execute(Autodesk.Revit.UI.ExternalCommandData CommandData, ref string Message, ElementSet elementset)
        {
            Autodesk.Revit.ApplicationServices.Application Revit = CommandData.Application.Application;
            Autodesk.Revit.UI.UIDocument uidoc = CommandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = CommandData.Application.ActiveUIDocument.Document;
            ICollection<ElementId> currentselectIdList = uidoc.Selection.GetElementIds();
            Autodesk.Revit.DB.View curview = doc.ActiveView;

            SelectByOriginIDForm form = new SelectByOriginIDForm();
            form.ShowDialog();
            if (form.DialogResult == DialogResult.Cancel)
                return Result.Succeeded;

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
                     //楼层
             BuiltInCategory.OST_Levels, 
                    //线
                    BuiltInCategory.OST_Lines,
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

            string originFileStr = form.textBox2.Text.Trim();
            List<string> originIdStrList = form.textBox1.Text.Split(new char[] { ',' }).ToList();

            ElementMulticategoryFilter mcf = new ElementMulticategoryFilter(categoryIdExceptList, true);
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<Element> elementsWithOriginID = collector
            .WhereElementIsNotElementType()
            .WherePasses(mcf)
            .Where(e => e.LookupParameter("OriginID") != null)
            .Where(e => originIdStrList.Contains(e.LookupParameter("OriginID").AsString())) // 过滤 OriginID 值
            .ToList();

            if (!string.IsNullOrWhiteSpace(originFileStr))
            {
                elementsWithOriginID = elementsWithOriginID.FindAll(e => e.LookupParameter("OriginFile").AsString() == originFileStr);
            }

            uidoc.Selection.SetElementIds(elementsWithOriginID.Select(e => e.Id).ToList());
            MessageBox.Show("已选择符合条件的构件共 " + elementsWithOriginID.Count + " 个。", "向日葵", MessageBoxButtons.OK);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
