using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using RevitAddIn.CommonUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.Commands
{
    /// <summary>
    /// 命令名：GenOriginFilePara，鼠标提示：自动对所选构件生成OriginFile、OriginTag参数值。逻辑是查找其相连构件，提取OriginFile非空参数值。
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class GenOriginFileParaCommand : IExternalCommand
    {
        public Result Execute(Autodesk.Revit.UI.ExternalCommandData CommandData, ref string Message, ElementSet elementset)
        {
            Autodesk.Revit.ApplicationServices.Application Revit = CommandData.Application.Application;
            Autodesk.Revit.UI.UIDocument uidoc = CommandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = CommandData.Application.ActiveUIDocument.Document;
            ICollection<ElementId> currentSelectIdList = uidoc.Selection.GetElementIds();
            Autodesk.Revit.DB.View curview = doc.ActiveView;

            if (currentSelectIdList.Count == 0)
            {
                MessageBox.Show("请先选择需生成OriginFile参数值的构件，再执行命令。可使用SelectNoOriginFile命令批量选择。", "向日葵", MessageBoxButtons.OK);
                return Result.Succeeded;
            }

            //把子图元去掉
            List<ElementId> subIdList = new List<ElementId>();
            foreach (ElementId id in currentSelectIdList)
            {
                if (doc.GetElement(id) is FamilyInstance fi)
                {
                    try
                    {
                        subIdList = subIdList.Union(fi.GetSubComponentIds()).ToList();
                    }
                    catch { }
                }
            }
            List<ElementId> selectIdList = currentSelectIdList.Except(subIdList).ToList();

            Categories cgs = doc.Settings.Categories;
            ElementId pfCGID = cgs.get_Item(BuiltInCategory.OST_PipeFitting).Id;
            ElementId paCGID = cgs.get_Item(BuiltInCategory.OST_PipeAccessory).Id;
            ElementId springklerCGID = cgs.get_Item(BuiltInCategory.OST_Sprinklers).Id;

            //先求所有MEP系统，在系统内传递参数
            ElementClassFilter psfilter = new ElementClassFilter(typeof(PipingSystem));
            IList<Element> pipingSystemList = new FilteredElementCollector(doc).WherePasses(psfilter).ToElements();
            Dictionary<Element, List<ElementId>> mepSystemElemIdSetDic = new Dictionary<Element, List<ElementId>>();
            foreach (Element element in pipingSystemList)
            {
                PipingSystem ps = element as PipingSystem;
                List<ElementId> ids = new List<ElementId>();
                foreach (Element e in ps.PipingNetwork)
                    ids.Add(e.Id);
                mepSystemElemIdSetDic.Add(ps, ids);
            }

            ElementClassFilter dsfilter = new ElementClassFilter(typeof(MechanicalSystem));
            IList<Element> ductSystemList = new FilteredElementCollector(doc).WherePasses(dsfilter).ToElements();

            foreach (Element element in ductSystemList)
            {
                MechanicalSystem ds = element as MechanicalSystem;
                List<ElementId> ids = new List<ElementId>();
                foreach (Element e in ds.DuctNetwork)
                    ids.Add(e.Id);
                mepSystemElemIdSetDic.Add(ds, ids);
            }

            HashSet<ElementId> hasValueIdHashSet = new HashSet<ElementId>();
            HashSet<ElementId> doneIdHashSet = new HashSet<ElementId>();
            int cal = 0;

            TransactionStatus status = TransactionStatus.Uninitialized;
            TransactionGroup transGroup = new TransactionGroup(doc, "参数赋值");
            if (transGroup.Start() == TransactionStatus.Started)
            {
                Transaction transaction = new Transaction(doc, "yy");
                transaction.Start();

                foreach (ElementId id in selectIdList)
                {
                    if (doneIdHashSet.Contains(id))
                        continue;
                    Element e = doc.GetElement(id);

                    //List<ElementId> test = MEPUtils.GetAllElementsLinkToMEP(e);
                    //MessageBox.Show(test.Count + "\n" + string.Join(",", test), "向日葵", MessageBoxButtons.OK);
                    //uidoc.Selection.SetElementIds(test);

                    string check = "";
                    try
                    {
                        check = e.LookupParameter("OriginFile").AsString();
                    }
                    catch
                    { }
                    if (!string.IsNullOrWhiteSpace(check))
                    {
                        hasValueIdHashSet.Add(id);
                        continue;
                    }

                    //先看看是否是其他Element的子图元，父图元有没有参数值
                    if (e is FamilyInstance fi)
                    {
                        if (fi.SuperComponent != null)
                        {
                            string checkParent = "";
                            try
                            {
                                checkParent = fi.SuperComponent.LookupParameter("OriginFile").AsString();
                            }
                            catch
                            { }
                            if (!string.IsNullOrWhiteSpace(checkParent))
                            {
                                e.LookupParameter("OriginFile").Set(checkParent);
                                e.LookupParameter("OriginTag").Set("NEW");
                                doneIdHashSet.Add(id);
                                cal++;
                                continue;
                            }
                            //再上一重
                            else if (fi.SuperComponent is FamilyInstance fipp && fipp.SuperComponent != null)
                            {
                                try
                                {
                                    checkParent = fipp.SuperComponent.LookupParameter("OriginFile").AsString();
                                }
                                catch
                                { }
                                if (!string.IsNullOrWhiteSpace(checkParent))
                                {
                                    e.LookupParameter("OriginFile").Set(checkParent);
                                    e.LookupParameter("OriginTag").Set("NEW");
                                    doneIdHashSet.Add(id);
                                    cal++;
                                    continue;
                                }
                            }
                        }
                    }

                    string valueStr = "";

                    List<ElementId> pseIdList = new List<ElementId>();
                    foreach (var pair in mepSystemElemIdSetDic)
                    {
                        if (pair.Value.Contains(id))
                        {
                            pseIdList = pair.Value;
                            foreach (ElementId pseId in pseIdList)
                            {
                                string tmp = "";
                                try
                                {
                                    tmp = doc.GetElement(pseId).LookupParameter("OriginFile").AsString();
                                }
                                catch
                                { }
                                if (!string.IsNullOrWhiteSpace(tmp))
                                {
                                    valueStr = tmp;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if (valueStr != "")
                    {
                        foreach (ElementId pseId in pseIdList)
                        {
                            if (doneIdHashSet.Contains(pseId))
                                continue;

                            if (currentSelectIdList.Contains(pseId))
                            {
                                doc.GetElement(pseId).LookupParameter("OriginFile").Set(valueStr);
                                doc.GetElement(pseId).LookupParameter("OriginTag").Set("NEW");
                                doneIdHashSet.Add(pseId);
                                cal++;

                                //子图元也加上
                                if (doc.GetElement(pseId) is FamilyInstance fi2)
                                {
                                    foreach (ElementId subId in fi2.GetSubComponentIds())
                                    {
                                        if (doneIdHashSet.Contains(subId))
                                            continue;
                                        doc.GetElement(subId).LookupParameter("OriginFile").Set(valueStr);
                                        doc.GetElement(subId).LookupParameter("OriginTag").Set("NEW");
                                        doneIdHashSet.Add(subId);
                                        //如果currentSelectIdList包含subid才计数
                                        if (currentSelectIdList.Contains(subId))
                                            cal++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        List<ElementId> linkIdList = MEPUtils.GetAllElementsLinkToMEP(e);
                        foreach (ElementId pseId in linkIdList)
                        {
                            string tmp = "";
                            try
                            {
                                tmp = doc.GetElement(pseId).LookupParameter("OriginFile").AsString();
                            }
                            catch
                            { }
                            if (!string.IsNullOrWhiteSpace(tmp))
                            {
                                valueStr = tmp;
                                break;
                            }
                        }

                        if (valueStr != "")
                        {
                            foreach (ElementId pseId in linkIdList)
                            {
                                if (doneIdHashSet.Contains(pseId))
                                    continue;

                                if (currentSelectIdList.Contains(pseId))
                                {
                                    doc.GetElement(pseId).LookupParameter("OriginFile").Set(valueStr);
                                    doc.GetElement(pseId).LookupParameter("OriginTag").Set("NEW");
                                    doneIdHashSet.Add(pseId);
                                    cal++;

                                    //子图元也加上
                                    if (doc.GetElement(pseId) is FamilyInstance fi2)
                                    {
                                        foreach (ElementId subId in fi2.GetSubComponentIds())
                                        {
                                            if (doneIdHashSet.Contains(subId))
                                                continue;
                                            doc.GetElement(subId).LookupParameter("OriginFile").Set(valueStr);
                                            doc.GetElement(subId).LookupParameter("OriginTag").Set("NEW");
                                            doneIdHashSet.Add(subId);
                                            //如果currentSelectIdList包含subid才计数
                                            if (currentSelectIdList.Contains(subId))
                                                cal++;
                                        }
                                    }
                                }
                            }
                        }

                    }



                    //    if (e is Pipe pipe)
                    //{
                    //    string valueStr = "";
                    //    //查找整个管道系统
                    //    PipingSystem pipingSystem = pipe.MEPSystem as PipingSystem;
                    //    ElementSet pipingNetwork = pipingSystem.PipingNetwork;
                    //    List<Element> toDoList = new List<Element>();
                    //    foreach (Element e2 in pipingNetwork)
                    //    {
                    //        string tmp = "";
                    //        try
                    //        {
                    //            tmp = e2.LookupParameter("OriginFile").AsString();
                    //        }
                    //        catch
                    //        { }
                    //        if (string.IsNullOrWhiteSpace(tmp))
                    //            toDoList.Add(e2);
                    //        else if (valueStr == "")
                    //            valueStr = tmp;
                    //    }
                    //    //如果找到有非空值
                    //    if (!string.IsNullOrWhiteSpace(valueStr))
                    //    {
                    //        e.LookupParameter("OriginFile").Set(valueStr);
                    //        e.LookupParameter("OriginTag").Set("NEW");
                    //        doneIdHashSet.Add(id);
                    //        cal++;
                    //        //顺便把整个管道系统未赋值的都一起赋了
                    //        foreach (Element eInSystem in toDoList)
                    //        {
                    //            if (currentSelectIdList.Contains(eInSystem.Id) && !doneIdHashSet.Contains(eInSystem.Id))
                    //            {
                    //                eInSystem.LookupParameter("OriginFile").Set(valueStr);
                    //                eInSystem.LookupParameter("OriginTag").Set("NEW");
                    //                doneIdHashSet.Add(eInSystem.Id);
                    //                cal++;
                    //            }
                    //        }
                    //    }
                    //    //如果没找到
                    //    else
                    //    {
                    //        ElementSet pipingNetwork01 = MEPUtils.pipelinkset(pipe, 0, pfCGID, paCGID);
                    //        ElementSet pipingNetwork02 = MEPUtils.pipelinkset(pipe, 1, pfCGID, paCGID);
                    //        foreach (Element ee in pipingNetwork02)
                    //            pipingNetwork01.Insert(ee);
                    //        List<Element> toDoList2 = new List<Element>();
                    //        foreach (Element e2 in pipingNetwork01)
                    //        {
                    //            string tmp = "";
                    //            try
                    //            {
                    //                tmp = e2.LookupParameter("OriginFile").AsString();
                    //            }
                    //            catch
                    //            { }
                    //            if (string.IsNullOrWhiteSpace(tmp))
                    //                toDoList2.Add(e2);
                    //            else if (valueStr == "")
                    //                valueStr = tmp;
                    //        }
                    //        //如果找到有非空值
                    //        if (!string.IsNullOrWhiteSpace(valueStr))
                    //        {
                    //            e.LookupParameter("OriginFile").Set(valueStr);
                    //            e.LookupParameter("OriginTag").Set("NEW");
                    //            doneIdHashSet.Add(id);
                    //            cal++;
                    //            //顺便把整个管道系统未赋值的都一起赋了
                    //            foreach (Element eInSystem in toDoList2)
                    //            {
                    //                if (currentSelectIdList.Contains(eInSystem.Id) && !doneIdHashSet.Contains(eInSystem.Id))
                    //                {
                    //                    eInSystem.LookupParameter("OriginFile").Set(valueStr);
                    //                    eInSystem.LookupParameter("OriginTag").Set("NEW");
                    //                    doneIdHashSet.Add(eInSystem.Id);
                    //                    cal++;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }

                transaction.Commit();
            }
            status = transGroup.Assimilate();

            if (hasValueIdHashSet.Count > 0)
                MessageBox.Show("已为" + cal + "个构件录入OriginFile、OriginTag参数。原选择集中" + hasValueIdHashSet.Count + "个构件原本已有OriginFile参数值，已跳过。", "uBIM Tools", MessageBoxButtons.OK);
            else
                MessageBox.Show("已为" + cal + "个构件录入OriginFile、OriginTag参数。", "uBIM Tools", MessageBoxButtons.OK);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
