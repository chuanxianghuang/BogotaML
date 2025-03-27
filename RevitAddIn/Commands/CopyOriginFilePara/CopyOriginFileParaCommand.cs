using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using RevitAddIn.UICommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.Commands.CopyOriginFilePara
{
    /// <summary>
    /// 命令名：CopyOriginFilePara，鼠标提示：将构件的OriginFile参数值复制到其他构件，同时复制或统一输入OriginTag参数。用于对新增构件进行参数补充。
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CopyOriginFileParaCommand : IExternalCommand
    {
        public Result Execute(Autodesk.Revit.UI.ExternalCommandData CommandData, ref string Message, ElementSet elementset)
        {
            ExecuteEventHandler executeEventHandler = new ExecuteEventHandler("BGT");
            ExternalEvent externalEvent = ExternalEvent.Create(executeEventHandler);
            CopyOriginFileParaForm form = new CopyOriginFileParaForm(CommandData, executeEventHandler, externalEvent);
            form.Show();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
