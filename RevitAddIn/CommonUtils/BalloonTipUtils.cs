using Autodesk.Internal.InfoCenter;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.CommonUtils
{
    public static class BalloonTipUtils
    {
        public static void ShowBalloonTip(string category, string title, string text)
        {
            ResultItem resultItem = new ResultItem
            {
                Category = category,
                Title = title,
                TooltipText = text,

                // Optional: provide a URL, e.g. a 
                // website containing further information.
                Uri = new System.Uri("http://www.youwebs.com"),
                IsFavorite = true,
                IsNew = true
            };

            // You also could add a click event.
            resultItem.ResultClicked += new EventHandler<ResultClickEventArgs>(ResultClicked);
            ComponentManager.InfoCenterPaletteManager.ShowBalloon(resultItem);
        }

        private static void ResultClicked(object sender, ResultClickEventArgs e)
        {
            Trace.TraceInformation("do some stuff...");
        }
    }
}
