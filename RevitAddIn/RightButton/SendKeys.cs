using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitAddIn.RightButton
{
    public static class SendKeyUtils
    {
        [DllImport("user32.dll")]
        //设置前置窗体
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        //发送按键
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);
        public static void SendKeys()
        {
            IntPtr Revit = Autodesk.Windows.ComponentManager.ApplicationWindow;
            PostMessage(Revit, 256U, 27U, 0U);
            //SetForegroundWindow(Revit);
            //for (int i = 0; i < count; i++)
            //{
            //    keybd_event((byte)keys, 0, 0, 0);
            //    keybd_event((byte)keys, 0, 2, 0);
            //}
        }


    }
}
