using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitAddIn.CommonUtils;
using RevitAddIn.RightButton;
using RevitAddIn.UICommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace RevitAddIn.Commands.CopyOriginFilePara
{
    public partial class CopyOriginFileParaForm : Form
    {
        private static CopyOriginFileParaForm _instance;
        private static readonly object _lock = new object();

        public static CopyOriginFileParaForm GetInstance(ExternalCommandData commandData, ExecuteEventHandler executeEventHandler, ExternalEvent externalEvent)
        {
            lock (_lock) // 确保线程安全
            {
                if (_instance == null || _instance.IsDisposed)
                {
                    _instance = new CopyOriginFileParaForm(commandData, executeEventHandler, externalEvent);
                }
                return _instance;
            }
        }

        ExecuteEventHandler _executeEventHandler = null;
        ExternalEvent _externalEvent = null;

        public Autodesk.Revit.ApplicationServices.Application Revit;
        public Autodesk.Revit.UI.UIDocument uidoc;
        public Autodesk.Revit.DB.Document doc;
        public List<ElementId> targetIdList = new List<ElementId>();
        public ElementId resouceId = ElementId.InvalidElementId;

        public CopyOriginFileParaForm(ExternalCommandData CommandData, ExecuteEventHandler executeEventHandler, ExternalEvent externalEvent)
        {
            InitializeComponent();

            _executeEventHandler = executeEventHandler;
            _externalEvent = externalEvent;

            Revit = CommandData.Application.Application;
            uidoc = CommandData.Application.ActiveUIDocument;
            doc = CommandData.Application.ActiveUIDocument.Document;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var hook = YIHook.GetInstance();
            hook.InstallAllHook();
            hook.OnMouseClick += new RightButtonUtils().MouseHook_OnMouseActivity;
            hook.OnKeyPress += new RightButtonUtils().KeyboardHook_OnSpaceActivity;

            IList<Reference> refers = new List<Reference>();
            try
            {
                refers = uidoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element,new SelectFilter());
                targetIdList = refers.Select(m => m.ElementId).ToList();
                label1.Text = "已选择 " + targetIdList.Count + " 个构件";
            }
            catch
            { }

            hook.OnMouseClick -= new RightButtonUtils().MouseHook_OnMouseActivity;
            hook.OnKeyPress -= new RightButtonUtils().KeyboardHook_OnSpaceActivity;
            hook.UninstallAllHook();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Reference refer = null;
            string originFileValue = "";

            try
            {
                refer = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, new SelectFilter());
                originFileValue = doc.GetElement(refer).LookupParameter("OriginFile").AsString();
            }
            catch
            { }

            if (string.IsNullOrWhiteSpace(originFileValue))
            {
                MessageBox.Show("所选构件没有OriginFile参数，请重新选择。", "向日葵", MessageBoxButtons.OK);
            }
            else
            {
                resouceId = refer.ElementId;
                label2.Text = "已选择构件，OriginFile参数值为：";
                label3.Text = originFileValue;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            this.DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (targetIdList.Count == 0)
            {
                MessageBox.Show("请选择需赋值的构件。", "向日葵", MessageBoxButtons.OK);
                return;
            }

            if (resouceId == ElementId.InvalidElementId)
            {
                MessageBox.Show("请选择提供参数值的构件。", "向日葵", MessageBoxButtons.OK);
                return;
            }

            string originFileValue = "";
            string originTagValue = "";
            try
            {
                originFileValue = doc.GetElement(resouceId).LookupParameter("OriginFile").AsString();
            }
            catch
            { }
            try
            {
                if (radioButton1.Checked)
                    originTagValue = doc.GetElement(resouceId).LookupParameter("OriginTag").AsString();
                else
                    originTagValue = textBox1.Text;
            }
            catch
            { }

            if (string.IsNullOrWhiteSpace(originFileValue))
            {
                MessageBox.Show("所选构件的OriginFile参数值为空，请重新选择。", "向日葵", MessageBoxButtons.OK);
                return;
            }
            if (radioButton1.Checked && string.IsNullOrWhiteSpace(originTagValue))
            {
                MessageBox.Show("所选构件的OriginTag参数值为空，请重新选择。", "向日葵", MessageBoxButtons.OK);
                return;
            }
            if (!radioButton1.Checked && string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("请设置OriginTag值。", "向日葵", MessageBoxButtons.OK);
                return;
            }

            if (_externalEvent != null)
            {
                _executeEventHandler.ExecuteAction = new Action<UIApplication>((app) =>
                {
                    TransactionStatus status = TransactionStatus.Uninitialized;
                    TransactionGroup transGroup = new TransactionGroup(doc, "参数赋值");
                    if (transGroup.Start() == TransactionStatus.Started)
                    {
                        Transaction transaction = new Transaction(doc, "yy");
                        transaction.Start();

                        foreach (ElementId id in targetIdList)
                        {
                            try
                            {
                                doc.GetElement(id).LookupParameter("OriginFile").Set(originFileValue);
                            }
                            catch
                            { }

                            try
                            {
                                doc.GetElement(id).LookupParameter("OriginTag").Set(originTagValue);
                            }
                            catch
                            { }
                        }

                        transaction.Commit();
                    }
                    status = transGroup.Assimilate();

                    MessageBox.Show("已为" + targetIdList.Count + "个构件录入OriginFile、OriginTag参数。", "uBIM Tools", MessageBoxButtons.OK);
                });
                _externalEvent.Raise();
            }

            //this.Tag = resouceId.ToString() + "," + string.Join(",", targetIdList);




            this.Close();
            this.DialogResult = DialogResult.OK;
        }
    }

    public class SelectUtils
    {


        public static List<ElementId> CategoryIdExceptList(Document doc)
        {
            List<ElementId> categoryIdExceptList = new List<ElementId>();
            Categories cgs = doc.Settings.Categories;

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
            return categoryIdExceptList;
        }
    }

    public class SelectFilter : ISelectionFilter
    {

        public bool AllowElement(Element elem)
        {
            if (elem.Category == null)
                return false;

            //if (SelectUtils.CategoryIdExceptList(elem.Document).Contains(elem.Category.Id))
            //    return false;
            else
                return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}
