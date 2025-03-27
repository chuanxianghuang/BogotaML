using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.CommonUtils
{
    public class MEPUtils
    {
        //取得与管件fm直接连接的管件集合
        public static ElementSet fm_linkto_fm_set(FamilyInstance fm, ElementId pipefittingCGID, ElementId pipeAccessoryCGID)
        {
            ElementSet fmlinkset = new ElementSet();
            ConnectorSetIterator csi = fm.MEPModel.ConnectorManager.Connectors.ForwardIterator();
            while (csi.MoveNext())
            {
                ConnectorSet connectorSet = (csi.Current as Connector).AllRefs;
                foreach (Connector ctmp in connectorSet)
                {
                    if (ctmp.Owner == null)
                        continue;
                    if (fmlinkset.Contains(ctmp.Owner) || ctmp.Owner.GetType().Name != "FamilyInstance")
                        continue;
                    ElementId id = (ctmp.Owner as FamilyInstance).Symbol.Family.FamilyCategory.Id;
                    if (id == pipefittingCGID || id == pipeAccessoryCGID)
                        fmlinkset.Insert(ctmp.Owner);
                }
            }
            return fmlinkset;
        }

        //取得与管件fm直接连接的管道集合
        public static ElementSet pipe_linkto_fm_set(FamilyInstance fm)
        {
            ElementSet pipelinkset = new ElementSet();
            ConnectorSetIterator csi = fm.MEPModel.ConnectorManager.Connectors.ForwardIterator();
            while (csi.MoveNext())
            {
                ConnectorSet connectorSet = (csi.Current as Connector).AllRefs;
                foreach (Connector ctmp in connectorSet)
                {
                    if (ctmp.Owner == null)
                        continue;
                    if (ctmp.Owner is Pipe && pipelinkset.Contains(ctmp.Owner) == false)
                        pipelinkset.Insert(ctmp.Owner);
                }
            }
            return pipelinkset;
        }

        //取得与管件fm直接或间接连接的管道集合
        public static ElementSet allpipe_linkto_fm_set(FamilyInstance fm, ElementId pipefittingCGID, ElementId pipeAccessoryCGID)
        {
            ElementSet allpipelinkset = pipe_linkto_fm_set(fm);  //直接连接的管道
            ElementSet fmlinkset = fm_linkto_fm_set(fm, pipefittingCGID, pipeAccessoryCGID);         //直接连接的管件
            foreach (Element e in fmlinkset)
            {
                FamilyInstance fm_tmp = e as FamilyInstance;
                foreach (Element e2 in fm_linkto_fm_set(fm_tmp, pipefittingCGID, pipeAccessoryCGID))
                {
                    if (!fmlinkset.Contains(e2))
                        fmlinkset.Insert(e2);
                }
            }
            foreach (Element e in fmlinkset)
            {
                FamilyInstance fm_tmp = e as FamilyInstance;
                foreach (Element e2 in pipe_linkto_fm_set(fm_tmp))
                {
                    if (!allpipelinkset.Contains(e2))
                        allpipelinkset.Insert(e2);
                }
            }
            return allpipelinkset;
        }

        //取得管道某端所连接的管道集合
        public static ElementSet pipelinkset(Pipe p, int i, ElementId pipefittingCGID, ElementId pipeAccessoryCGID)
        {
            ElementSet pipelinkset = new ElementSet();
            ConnectorSetIterator csi = p.ConnectorManager.Connectors.ForwardIterator();
            while (csi.MoveNext())
            {
                LocationCurve lc = p.Location as LocationCurve;
                if ((csi.Current as Connector).Origin.DistanceTo(lc.Curve.GetEndPoint(i)) > 0.05)
                    continue;
                ConnectorSet connectorSet = (csi.Current as Connector).AllRefs;    //csi.Current as Connector为所需的管端connector
                foreach (Connector cc in connectorSet)    //cc.owner为管件
                {
                    if (cc.Owner == null)
                        continue;
                    if (cc.Owner is Pipe)
                    {
                        Pipe pipe = cc.Owner as Pipe;
                        if (pipe.Id == p.Id)
                            continue;
                        if (pipe != null)
                            pipelinkset.Insert(pipe);
                    }
                    if (cc.Owner is FamilyInstance)
                    {
                        FamilyInstance fm = cc.Owner as FamilyInstance;
                        if (fm == null)
                            continue;
                        ElementId fmCGID = fm.Symbol.Family.FamilyCategory.Id;
                        if (fmCGID != pipefittingCGID && fmCGID != pipeAccessoryCGID)
                            continue;
                        foreach (Element e in allpipe_linkto_fm_set(fm, pipefittingCGID, pipeAccessoryCGID))
                        {
                            if (e.Id != p.Id && !pipelinkset.Contains(e))
                                pipelinkset.Insert(e);
                        }
                    }
                }
            }
            return pipelinkset;
        }

        /// <summary>
        /// 选择与构件直接通过Connector连接的构件集合
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<ElementId> GetElementsLinkToMEP(Element e)
        {
            List<ElementId> idList = new List<ElementId>();
            idList.Add(e.Id);
            ConnectorManager cm = null;
            if (e is CableTray ct)
                cm = ct.ConnectorManager;
            else if (e is Conduit conduit)
                cm = conduit.ConnectorManager;
            else if (e is Pipe pipe)
                cm = pipe.ConnectorManager;
            else if (e is FlexPipe flexPipe)
                cm = flexPipe.ConnectorManager;
            else if (e is Duct duct)
                cm = duct.ConnectorManager;
            else if (e is FlexDuct flexDuct)
                cm = flexDuct.ConnectorManager;
            else if (e is FamilyInstance fi)
                cm = fi.MEPModel.ConnectorManager;

            if (cm != null && cm.Connectors.Size > 0)
            {
                foreach (Connector con in cm.Connectors)
                {
                    foreach (Connector connec in con.AllRefs)
                    {
                        if (connec.Owner is MEPSystem)
                            continue;
                        if (!idList.Contains(connec.Owner.Id))
                            idList.Add(connec.Owner.Id);
                    }
                }
            }

            return idList;
        }

        /// <summary>
        /// 求MEP链路的所有构件
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<ElementId> GetAllElementsLinkToMEP(Element e)
        {
            List<ElementId> idList = new List<ElementId>();
            List<ElementId> doneList = new List<ElementId>();

            idList.Add(e.Id);
            bool add = false;
            int cal = 0;
            do
            {
                cal++;
                add = false;
                List<ElementId> addList = new List<ElementId>();
                foreach (ElementId id in idList)
                {
                    if (doneList.Contains(id))
                        continue;
                    List<ElementId> idListTmp = GetElementsLinkToMEP(e.Document.GetElement(id));
                    foreach (ElementId idTmp in idListTmp)
                    {
                        if (!idList.Contains(idTmp))
                            addList.Add(idTmp);
                    }
                    doneList.Add(id);
                }

                foreach (ElementId id in addList)
                {
                    idList.Add(id);
                    add = true;
                }
            }
            while (add && cal < 10000);

            idList = idList.Distinct().ToList();

            return idList;
        }
    }
}
