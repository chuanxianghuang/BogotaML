using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace RevitAddIn.Commands.GroupByOriginFile
{
    /// <summary>
    /// 命令名： 命令名：GroupByOriginFile，鼠标提示：根据不同的OriginFile参数值分别建立3D视图，并可分别导出NWC。
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class GroupByOriginFileCommand : IExternalCommand
    {
        public Result Execute(Autodesk.Revit.UI.ExternalCommandData CommandData, ref string Message, ElementSet elementset)
        {
            Autodesk.Revit.ApplicationServices.Application Revit = CommandData.Application.Application;
            Autodesk.Revit.UI.UIDocument uidoc = CommandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = CommandData.Application.ActiveUIDocument.Document;
            ICollection<ElementId> currentselectIdList = uidoc.Selection.GetElementIds();
            Autodesk.Revit.DB.View curview = doc.ActiveView;

            GroupByOriginFileForm form = new GroupByOriginFileForm();

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
            .ToList();

            var groups = elementsWithOriginFile.GroupBy(m => m.LookupParameter("OriginFile").AsString());

            foreach (var group in groups)
            {
                string originFileName = group.Key;
                if (string.IsNullOrWhiteSpace(originFileName))
                    form.dataGridView1.Rows.Add("NoOriginFileName", group.ToList().Count);
                else
                    form.dataGridView1.Rows.Add(originFileName, group.ToList().Count);
            }
            form.ShowDialog();
            if (form.DialogResult == DialogResult.Cancel)
                return Result.Succeeded;

            Transaction transaction = new Transaction(doc, "创建视图");
            transaction.Start();

            List<ElementId> view3DIdList = new List<ElementId>();
            foreach (var group in groups)
            {
                string originFileName = group.Key;
                if (string.IsNullOrWhiteSpace(originFileName))
                    originFileName = "NoOriginFileName";
                List<Element> elements = group.ToList();
                if (string.IsNullOrEmpty(originFileName))
                    continue;

                View3D view3D = Create3DView(doc);
                view3D.Name = originFileName + "_" + DateTime.Now.ToString("yyyyMMdd-HHMM");
                ElementId filterId = CreateOriginFileFilter(doc, originFileName, "OriginFile", originFileName);
                ApplyFilterToView(doc, view3D, filterId);

                #region 关闭MEP中心线
                view3D.SetCategoryHidden(doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeCurvesCenterLine).Id, true);
                view3D.SetCategoryHidden(doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeFittingCenterLine).Id, true);
                view3D.SetCategoryHidden(doc.Settings.Categories.get_Item(BuiltInCategory.OST_DuctCurvesCenterLine).Id, true);
                view3D.SetCategoryHidden(doc.Settings.Categories.get_Item(BuiltInCategory.OST_DuctFittingCenterLine).Id, true);
                view3D.SetCategoryHidden(doc.Settings.Categories.get_Item(BuiltInCategory.OST_CableTrayCenterLine).Id, true);
                view3D.SetCategoryHidden(doc.Settings.Categories.get_Item(BuiltInCategory.OST_CableTrayFittingCenterLine).Id, true);
                #endregion

                view3D.DisplayStyle = DisplayStyle.Shading;
                view3D.DetailLevel = ViewDetailLevel.Fine;


                view3DIdList.Add(view3D.Id);
            }

            transaction.Commit();

            if (form.Tag.ToString() == "NWC")
            {
                foreach (ElementId id in view3DIdList)
                {
                    View3D view3D = doc.GetElement(id) as View3D;
                    uidoc.ActiveView = view3D;

                    ViewSet viewSet = new ViewSet();
                    viewSet.Insert(view3D);

                    uidoc.ActiveView = view3D;

                    string nwcName = view3D.Name + ".nwc";
                    string nwcPath = doc.PathName.Substring(0, doc.PathName.LastIndexOf("\\")) + "\\" + nwcName;

                    NavisworksExportOptions nwcOption = new NavisworksExportOptions();
                    nwcOption.Parameters = NavisworksParameters.Elements;
                    nwcOption.ExportLinks = false;
                    nwcOption.ConvertElementProperties = true;
                    nwcOption.Coordinates = NavisworksCoordinates.Shared;
                    nwcOption.ExportRoomGeometry = false;
                    nwcOption.ExportScope = NavisworksExportScope.View;
                    nwcOption.ViewId = view3D.Id;

                    doc.Export(doc.PathName.Substring(0, doc.PathName.LastIndexOf("\\")), nwcName, nwcOption);
                }
            }

            //uidoc.Selection.SetElementIds(elementsWithOriginFile.Select(e => e.Id).ToList());
            MessageBox.Show("已选择没有OriginFile参数或参数值为空的构件共 " + elementsWithOriginFile.Count + " 个。", "向日葵", MessageBoxButtons.OK);

            return Autodesk.Revit.UI.Result.Succeeded;
        }

        private View3D Create3DView(Document doc)
        {
            // 获取当前视图
            ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);

            if (viewFamilyType == null)
            {
                TaskDialog.Show("Error", "无法找到3D视图类型");
                return null;
            }

            // 创建一个新的3D视图
            View3D view3D = View3D.CreateIsometric(doc, viewFamilyType.Id);

            return view3D;
        }

        // 创建OriginFile参数过滤器
        private ElementId CreateOriginFileFilter(Document doc, string filterName, string paramName, string targetValue)
        {
            // 获取目标参数定义
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(SharedParameterElement));

            Element paramElement = collector.FirstOrDefault(e => e.Name.Equals(paramName));

            // 定义过滤规则
            FilterRule rule = ParameterFilterRuleFactory.CreateEqualsRule(
                new ElementId(paramElement.Id.IntegerValue),
                targetValue,
                caseSensitive: false
            );

            // 定义目标类别
            List<BuiltInCategory> targetCategories = new List<BuiltInCategory>
        {
            BuiltInCategory.OST_Walls,         // 墙
            BuiltInCategory.OST_StructuralColumns,  // 柱
            BuiltInCategory.OST_StructuralFraming,  // 梁
            BuiltInCategory.OST_Floors         // 板
        };

            // 创建过滤器
            IList<ElementId> categoryIds = new List<ElementId>();
            foreach (BuiltInCategory bic in targetCategories)
            {
                categoryIds.Add(new ElementId((int)bic));
            }

            //ElementFilter ef = ElementFilter
            ElementParameterFilter elementFilter = new ElementParameterFilter(rule);

            ParameterFilterElement filter = ParameterFilterElement.Create(
                doc,
                filterName,
                categoryIds,
                elementFilter
            );

            return filter.Id;
        }

        // 将过滤器应用到视图
        private void ApplyFilterToView(Document doc, View3D view, ElementId filterId)
        {
            // 设置覆盖图形样式（可选）
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            ogs.SetProjectionLineColor(new Autodesk.Revit.DB.Color(128, 128, 128)); // 灰色

            // 添加过滤器到视图
            view.AddFilter(filterId);
            view.SetFilterOverrides(filterId, ogs);
        }

        //private void ApplyCategoryFilter(View3D view3D, Document doc)
        //{
        //    // 获取所有墙、柱、梁、板的类别
        //    BuiltInCategory[] categories = {
        //    BuiltInCategory.OST_Walls,
        //    BuiltInCategory.OST_StructuralColumns,
        //    BuiltInCategory.OST_StructuralFraming,
        //    BuiltInCategory.OST_Floors
        //};

        //    // 对每个类别应用过滤器
        //    foreach (BuiltInCategory category in categories)
        //    {
        //        Category cat = doc.Settings.Categories.get_Item(category);

        //        if (cat != null)
        //        {
        //            // 为视图设置该类别的可见性
        //            view3D.SetCategoryVisibility(cat.Id, true);
        //        }
        //    }
        //}
    }
}
