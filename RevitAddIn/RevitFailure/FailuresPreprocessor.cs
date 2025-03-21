using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.RevitFailure
{
    public static class FailuresPreprocessor
    {
        internal static FailureDefinitionId CheckMEPSystemId = new FailureDefinitionId(new Guid("15060356-048c-4298-8ad8-a34c1637d45f"));
        public static void ProcessorFailuresMessage(Document doc, FailureDefinitionId fid, List<ElementId> invalids)
        {
            //#if DEBUG
            //            foreach (var item in invalids)
            //            {
            //                Trace.TraceInformation("invalid=" + item);
            //            }
            //#else
            using Transaction trans = new Transaction(doc, "警告提示用户");
            

            var fho = trans.GetFailureHandlingOptions();
            fho.SetForcedModalHandling(false);
            fho.SetDelayedMiniWarnings(true);
           
            trans.SetFailureHandlingOptions(fho);
            
            var failureMessage = new FailureMessage(fid);
            failureMessage.SetFailingElements(invalids);

            trans.Start();
            doc.PostFailure(failureMessage);
           
            trans.Commit();
            //trans.Dispose();
            //#endif
        }
    }
}
