using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DejaVuLib
{
    public class MouseHook : IDisposable
    {
        private Win32.CallBackHandler callBackHandler;
        private IntPtr hookID = IntPtr.Zero;

        public event EventHandler<ComputerEvent> OnMouseEvent;

        public MouseHook()
        {
            callBackHandler = HookCallback;
        }

        public void HookMouse()
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            using (ProcessModule currentModule = currentProcess.MainModule)
            {
                hookID = Win32.SetWindowsHookEx(Win32.WH_MOUSE_LL, callBackHandler, Win32.GetModuleHandle(currentModule.ModuleName), 0);
            }
        }

        public void Dispose()
        {
            Win32.UnhookWindowsHookEx(hookID);
        }
        
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && OnMouseEvent != null)
            {
                if (wParam == (IntPtr)Win32.WM_LBUTTONDOWN)
                {
                    OnMouseEvent(this, new LeftMouseDown());
                }
                else if (wParam == (IntPtr)Win32.WM_LBUTTONUP)
                {
                    OnMouseEvent(this, new LeftMouseUp());
                }
                else if (wParam == (IntPtr)Win32.WM_RBUTTONDOWN)
                {
                    OnMouseEvent(this, new RightMouseDown());
                }
                else if (wParam == (IntPtr)Win32.WM_RBUTTONUP)
                {
                    OnMouseEvent(this, new RightMouseUp());
                }
                else if (wParam == (IntPtr)Win32.WM_MOUSEMOVE)
                {
                    Win32.MSLHOOKSTRUCT hookStruct = (Win32.MSLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.MSLHOOKSTRUCT));
                    OnMouseEvent(this, new MouseMove(hookStruct.pt.x, hookStruct.pt.y));
                }
                else if (wParam == (IntPtr)Win32.WM_MOUSEWHEEL)
                {
                    Win32.MSLHOOKSTRUCT hookStruct = (Win32.MSLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.MSLHOOKSTRUCT));
                    OnMouseEvent(this, new MouseWheel(hookStruct.mouseData));
                }
                else if (wParam == (IntPtr)Win32.WM_MBUTTONDOWN)
                {
                    OnMouseEvent(this, new MiddleMouseDown());
                }
                else if (wParam == (IntPtr)Win32.WM_MBUTTONUP)
                {
                    OnMouseEvent(this, new MiddleMouseUp());
                }
                else if (wParam == (IntPtr)Win32.WM_XBUTTONDOWN)
                {
                    Win32.MSLHOOKSTRUCT hookStruct = (Win32.MSLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.MSLHOOKSTRUCT));

                    UInt16 mouseDataHighOrderWord = (UInt16)(hookStruct.mouseData >> 16);

                    if (mouseDataHighOrderWord == Win32.XBUTTON2)
                    {
                        OnMouseEvent(this, new ForwardMouseDown());
                    }
                    else if (mouseDataHighOrderWord == Win32.XBUTTON1)
                    {
                        OnMouseEvent(this, new BackMouseDown());
                    }
                }
                else if (wParam == (IntPtr)Win32.WM_XBUTTONUP)
                {
                    Win32.MSLHOOKSTRUCT hookStruct = (Win32.MSLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.MSLHOOKSTRUCT));
                    UInt16 mouseDataHighOrderWord = (UInt16)(hookStruct.mouseData >> 16);

                    if (mouseDataHighOrderWord == Win32.XBUTTON2)
                    {
                        OnMouseEvent(this, new ForwardMouseUp());
                    }
                    else if (mouseDataHighOrderWord == Win32.XBUTTON1)
                    {
                        OnMouseEvent(this, new BackMouseUp());
                    }
                }
            }
            return Win32.CallNextHookEx(hookID, nCode, wParam, lParam);
        }
    }
}
