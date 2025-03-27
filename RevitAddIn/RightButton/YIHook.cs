using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace RevitAddIn.RightButton
{
    public class YIHook
    {
        #region WIN32方法
        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(HookType idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);
        #endregion

        #region 键盘事件
        public event KeyEventHandler OnKeyDown;


        public event KeyEventHandler OnKeyUp;


        public event KeyEventHandler OnKeyPress;
        #endregion

        #region 鼠标事件
        public event MouseEventHandler OnMouseClick;


        public event MouseEventHandler OnMouseDoubleClick;


        public event MouseEventHandler OnMouseDown;


        public event MouseEventHandler OnMouseMove;

        public event MouseEventHandler OnMouseUp;
        #endregion

        #region 静态变量
        static YIHook HookInstance;

        public const int WH_MOUSE_LL = 14;

        public const int WH_MOUSE = 7;

        //键盘钩子Id
        private static int kbhHook;

        //鼠标钩子Id
        private static int mshHook;

        //键盘钩子回调
        private static HookProc kbHookProc;

        //鼠标钩子回调函数
        private static HookProc msHookProc;

        #endregion

        private YIHook()
        {

        }

        /// <summary>
        /// 获取钩子实例
        /// </summary>
        /// <returns></returns>
        public static YIHook GetInstance()
        {
            if (HookInstance == null)
            {
                HookInstance = new YIHook();
            }
            return HookInstance;
        }

        ~YIHook()
        {
            //UninstallAllHook();
        }

        #region 注册钩子
        /// <summary>
        /// 注册键盘钩子
        /// </summary>
        public void InstallKeyBoardHook()
        {
            InstallKeyBoardHookImpl(HookType.WH_KEYBOARD_LL);
        }

        /// <summary>
        /// 注册鼠标钩子
        /// </summary>
        public void InstallMouseHook()
        {
            InstallMouseHookImpl(HookType.WH_MOUSE_LL);
        }

        /// <summary>
        /// 注册所有钩子
        /// </summary>
        public void InstallAllHook()
        {
            InstallKeyBoardHook();
            InstallMouseHook();
        }

        private void InstallKeyBoardHookImpl(HookType type)
        {
            if (kbhHook == 0)
            {
                kbHookProc = new HookProc(DefaultKeyBoardHookProc);
                kbhHook = SetWindowsHookEx(type, kbHookProc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                if (kbhHook == 0)
                {
                    UninstallKeyBoardHook();
                }
            }
        }

        private void InstallMouseHookImpl(HookType type)
        {
            try
            {
                if (mshHook == 0)
                {
                    msHookProc = new HookProc(DefaultMouseHookProc);
                    mshHook = SetWindowsHookEx(type, msHookProc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                    Debug.WriteLine("InstallMouseHook nshHook" + mshHook);
                    if (mshHook == 0)
                    {
                        UninstallMouseHook();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("InstallMouseHook failed." + ex.Message);
            }
        }

        #endregion

        #region 卸载钩子
        public void UninstallKeyBoardHook()
        {
            UninstallHookImpl(ref kbhHook);
            Debug.WriteLine("UninstallkbhHook:" + kbhHook);
        }

        public void UninstallMouseHook()
        {
            UninstallHookImpl(ref mshHook);
            Debug.WriteLine("UninstallnshHook:" + mshHook);
        }

        public void UninstallAllHook()
        {
            UninstallKeyBoardHook();
            UninstallMouseHook();
            //this.UninstallHookImpl(ref YIHook.userHook);
            //Debug.WriteLine("UninstalluserHook:" + userHook);
        }

        private void UninstallHookImpl(ref int idhook)
        {
            if (idhook != 0)
            {
                UnhookWindowsHookEx(idhook);
                idhook = 0;
            }
        }
        #endregion

        private int DefaultKeyBoardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            KBDLLHOOKSTRUCT kbdllhookstruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
            if (wParam == 256 && OnKeyDown != null)
            {
                OnKeyDown.Invoke(this, new KeyEventArgs((Keys)kbdllhookstruct.vkCode));
            }
            else
            {
                if (wParam == 257 && OnKeyUp != null)
                {
                    OnKeyUp.Invoke(this, new KeyEventArgs((Keys)kbdllhookstruct.vkCode));
                }
                if (wParam == 257 && OnKeyPress != null)
                {
                    OnKeyPress.Invoke(this, new KeyEventArgs((Keys)kbdllhookstruct.vkCode));
                }
            }
            return CallNextHookEx(kbhHook, nCode, wParam, lParam);
        }

        private int DefaultMouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            MOUSEHOOKSTRUCT mousehookstruct = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));
            MouseButtons button = MouseButtons.None;
            int idHook = 0;
            switch (wParam)
            {
                case 512:
                    InvokeMouseEvent(OnMouseMove, button, idHook, mousehookstruct.pt);
                    break;
                case 513:
                    InvokeMouseEvent(OnMouseDown, MouseButtons.Left, 1, mousehookstruct.pt);
                    break;
                case 514:
                    InvokeMouseEvent(OnMouseClick, MouseButtons.Left, 1, mousehookstruct.pt);
                    InvokeMouseEvent(OnMouseUp, MouseButtons.Left, 1, mousehookstruct.pt);
                    break;
                case 515:
                    InvokeMouseEvent(OnMouseDoubleClick, MouseButtons.Left, 2, mousehookstruct.pt);
                    break;
                case 516:
                    InvokeMouseEvent(OnMouseDown, MouseButtons.Right, 1, mousehookstruct.pt);
                    return 1;
                case 517:
                    InvokeMouseEvent(OnMouseClick, MouseButtons.Right, 1, mousehookstruct.pt);
                    InvokeMouseEvent(OnMouseUp, MouseButtons.Right, 1, mousehookstruct.pt);
                    return 1;
                case 518:
                    InvokeMouseEvent(OnMouseDoubleClick, MouseButtons.Right, 2, mousehookstruct.pt);
                    break;
            }
            return CallNextHookEx(mshHook, nCode, wParam, lParam);
        }

        //触发鼠标操作
        private void InvokeMouseEvent(Delegate eventName, MouseButtons button, int clickCount, POINT p)
        {
            if (eventName == null)
            {
                return;
            }
            MouseEventArgs mouseEventArgs = new MouseEventArgs(button, clickCount, p.x, p.y, 0);
            eventName.DynamicInvoke(new object[]
            {
                this,
                mouseEventArgs
            });
        }

        public static void Dispose()
        {
            HookInstance = null;

            kbHookProc = null;

            msHookProc = null;
        }

        //回调委托
        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);

        #region 消息类
        public enum HookType
        {
            WH_MSGFILTER = -1,
            WH_JOURNALRECORD,
            WH_JOURNALPLAYBACK,
            WH_KEYBOARD,
            WH_GETMESSAGE,
            WH_CALLWNDPROC,
            WH_CBT,
            WH_SYSMSGFILTER,
            WH_MOUSE,
            WH_HARDWARE,
            WH_DEBUG,
            WH_SHELL,
            WH_FOREGROUNDIDLE,
            WH_CALLWNDPROCRET,
            WH_KEYBOARD_LL,
            WH_MOUSE_LL
        }

        public enum MsgType : uint
        {
            WM_KEYFIRST = 256U,
            WM_KEYDOWN = 256U,
            WM_KEYUP,
            WM_CHAR,
            WM_DEADCHAR,
            WM_SYSKEYDOWN,
            WM_SYSKEYUP,
            WM_SYSCHAR,
            WM_SYSDEADCHAR,
            WM_INITDIALOG = 272U,
            WM_COMMAND,
            WM_SYSCOMMAND,
            WM_TIMER,
            WM_HSCROLL,
            WM_VSCROLL,
            WM_INITMENU,
            WM_INITMENUPOPUP,
            WM_MENUSELECT = 287U,
            WM_MENUCHAR,
            WM_ENTERIDLE,
            WM_CTLCOLORMSGBOX = 306U,
            WM_CTLCOLOREDIT,
            WM_CTLCOLORLISTBOX,
            WM_CTLCOLORBTN,
            WM_CTLCOLORDLG,
            WM_CTLCOLORSCROLLBAR,
            WM_CTLCOLORSTATIC,
            WM_MOUSEWHEEL = 522U,
            WM_MBUTTONDBLCLK = 521U,
            WM_MBUTTONUP = 520U,
            WM_MOUSEMOVE = 512U,
            WM_LBUTTONDOWN,
            WM_LBUTTONUP,
            WM_LBUTTONDBLCLK,
            WM_RBUTTONDOWN,
            WM_RBUTTONUP,
            WM_RBUTTONDBLCLK,
            WM_MBUTTONDOWN,
            WM_CREATE = 1U,
            WM_DESTROY,
            WM_MOVE,
            WM_SIZE = 5U,
            WM_ACTIVATE,
            WM_SETFOCUS,
            WM_KILLFOCUS,
            WM_ENABLE = 10U,
            WM_SETREDRAW,
            WM_SETTEXT,
            WM_GETTEXT,
            WM_GETTEXTLENGTH,
            WM_PAINT,
            WM_CLOSE,
            WM_QUERYENDSESSION,
            WM_QUIT,
            WM_QUERYOPEN,
            WM_ERASEBKGND,
            WM_SYSCOLORCHANGE,
            WM_ENDSESSION,
            WM_SHOWWINDOW = 24U,
            WM_ACTIVATEAPP = 28U,
            WM_FONTCHANGE,
            WM_TIMECHANGE,
            WM_CANCELMODE,
            WM_SETCURSOR,
            WM_MOUSEACTIVATE,
            WM_CHILDACTIVATE,
            WM_QUEUESYNC,
            WM_GETMINMAXINFO,
            WM_PAINTICON = 38U,
            WM_ICONERASEBKGND,
            WM_NEXTDLGCTL,
            WM_SPOOLERSTATUS = 42U,
            WM_DRAWITEM,
            WM_MEASUREITEM,
            WM_VKEYTOITEM = 46U,
            WM_CHARTOITEM,
            WM_SETFONT,
            WM_GETFONT,
            WM_SETHOTKEY,
            WM_GETHOTKEY,
            WM_QUERYDRAGICON = 55U,
            WM_COMPAREITEM = 57U,
            WM_COMPACTING = 65U,
            WM_WINDOWPOSCHANGING = 70U,
            WM_WINDOWPOSCHANGED,
            WM_POWER,
            WM_COPYDATA = 74U,
            WM_CANCELJOURNA,
            WM_NOTIFY = 78U,
            WM_INPUTLANGCHANGEREQUEST = 80U,
            WM_INPUTLANGCHANGE,
            WM_TCARD,
            WM_HELP,
            WM_USERCHANGED,
            WM_NOTIFYFORMAT,
            WM_CONTEXTMENU = 123U,
            WM_STYLECHANGING,
            WM_STYLECHANGED,
            WM_DISPLAYCHANGE,
            WM_GETICON,
            WM_SETICON,
            WM_NCCREATE,
            WM_NCDESTROY,
            WM_NCCALCSIZE,
            WM_NCHITTEST,
            WM_NCPAINT,
            WM_NCACTIVATE,
            WM_GETDLGCODE,
            WM_NCMOUSEMOVE = 160U,
            WM_NCLBUTTONDOWN,
            WM_NCLBUTTONUP,
            WM_NCLBUTTONDBLCLK,
            WM_NCRBUTTONDOWN,
            WM_NCRBUTTONUP,
            WM_NCRBUTTONDBLCLK,
            WM_NCMBUTTONDOWN,
            WM_NCMBUTTONUP,
            WM_NCMBUTTONDBLCLK
        }

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public POINT()
            {
            }

            public int x;

            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MOUSEHOOKSTRUCT
        {
            public MOUSEHOOKSTRUCT()
            {
            }

            public POINT pt;

            public IntPtr hWnd;

            public int wHitTestCode;

            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            public KBDLLHOOKSTRUCT()
            {

            }

            public int vkCode;

            public int scanCode;

            public int flags;

            public int time;

            public int dwExtraInfo;
        }
        #endregion

        #region 废弃代码

        //public int InstallHook(YIHook.HookType type, YIHook.HookProc dele)
        //{
        //    if (YIHook.userHook == 0)
        //    {
        //        YIHook.userHook = YIHook.SetWindowsHookEx(type, dele, YIHook.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
        //        if (YIHook.userHook == 0)
        //        {
        //            this.UninstallKeyBoardHook();
        //        }
        //    }
        //    return YIHook.userHook;
        //}

        //public void InstallHook(YIHook.HookType type, YIHook.HookProc dele, IntPtr moduleHandel, int threadId)
        //{
        //    try
        //    {
        //        if (YIHook.userHook == 0)
        //        {
        //            YIHook.userHook = YIHook.SetWindowsHookEx(type, dele, moduleHandel, threadId);
        //            if (YIHook.userHook == 0)
        //            {
        //                this.UninstallKeyBoardHook();
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //} 


        #endregion

    }


}


