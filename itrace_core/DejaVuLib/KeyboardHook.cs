﻿/********************************************************************************************************************************************************
* @file KeyboardHook.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace iTrace_Core
{
    public class KeyboardHook : IDisposable
    {
        private Win32.CallBackHandler callBackHandler;
        private IntPtr hookID = IntPtr.Zero;

        public event EventHandler<ComputerEvent> OnKeyboardEvent;

        public KeyboardHook()
        {
            callBackHandler = HookCallback;
        }

        public void HookKeyboard()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                hookID = Win32.SetWindowsHookEx(Win32.WH_KEYBOARD_LL, callBackHandler, Win32.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public void Dispose()
        {
            Win32.UnhookWindowsHookEx(hookID);
        }
            
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if ((wParam == (IntPtr)Win32.WM_KEYDOWN || wParam == (IntPtr)Win32.WM_SYSKEYDOWN))
                {
                    byte virtualKeyCode = Marshal.ReadByte(lParam);

                    OnKeyboardEvent(this, new KeyDown(virtualKeyCode));
                }
                else
                {
                    byte virtualKeyCode = Marshal.ReadByte(lParam);

                    OnKeyboardEvent(this, new KeyUp(virtualKeyCode));
                }
            }

            return Win32.CallNextHookEx(hookID, nCode, wParam, lParam);
        }
    }
}
