using RevitAddIn.RightButton;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddIn.CommonUtils
{
    public class RightButtonUtils
    {
        public void MouseHook_OnMouseActivity(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.Clicks == 1)
            {
                CompleteMultiSelection();
            }
        }

        public void KeyboardHook_OnSpaceActivity(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                CompleteMultiSelection();
            }
        }

        public void CompleteMultiSelection()
        {
            var rvtwindow = Autodesk.Windows.ComponentManager.ApplicationWindow;
            var list = new List<IntPtr>();
            var flag = WindowsHelper.EnumChildWindows(rvtwindow,
                       (hwnd, l) =>
                       {
                           StringBuilder windowText = new StringBuilder(200);
                           WindowsHelper.GetWindowText(hwnd, windowText, windowText.Capacity);
                           StringBuilder className = new StringBuilder(200);
                           WindowsHelper.GetClassName(hwnd, className, className.Capacity);
                           if ((windowText.ToString().Equals("完成", StringComparison.Ordinal) ||
                          windowText.ToString().Equals("Finish", StringComparison.Ordinal)) &&
                          className.ToString().Contains("Button"))
                           {
                               list.Add(hwnd);
                               return false;
                           }
                           return true;
                       }, new IntPtr(0));

            var complete = list.FirstOrDefault();
            WindowsHelper.SendMessage(complete, 245, 0, 0);
            Debug.WriteLine(complete.ToString());
        }
    }
}
