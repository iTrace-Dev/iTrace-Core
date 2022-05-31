/********************************************************************************************************************************************************
* @file WindowPositionManager.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iTrace_Core
{
    public class WindowPositionManager
    {
        private Thread forcerThread;

        private HashSet<IntPtr> handles;
        private HashSet<IntPtr> refreshedHandles;

        public WindowPositionManager()
        {
            handles = new HashSet<IntPtr>();
            refreshedHandles = new HashSet<IntPtr>();
        }

        public void Start()
        {
            Win32.EnumWindows(InitialSeedCallback, 0);
            StartForcingFixedStartLocation();
        }

        public void Stop()
        {
            if(forcerThread != null)
            {
                forcerThread.Abort();
            }
        }

        private bool MoveIfNotInSetCallback(IntPtr hWnd, int lParam)
        {
            refreshedHandles.Add(hWnd);

            if (!handles.Contains(hWnd))
            {
                handles.Add(hWnd);

                new Thread(new ThreadStart(() =>
                {
                    Thread.Sleep(200);

                    var info = new Win32.WindowInfo();

                    if (Win32.GetWindowInfo(hWnd, out info))
                    {
                        bool maximizable = (info.dwStyle & Win32.WS_MAXIMIZEBOX) != 0;
                        bool minimizable = (info.dwStyle & Win32.WS_MINIMIZEBOX) != 0;

                        if (!maximizable && minimizable)
                        {
                            int height = Convert.ToInt32(info.rcWindow.Bottom - info.rcWindow.Top);
                            int width = Convert.ToInt32(info.rcWindow.Right - info.rcWindow.Left);

                            Win32.MoveWindow(hWnd, 10, 10, width, height, false);
                        }
                        else if (minimizable)
                        {
                            Win32.MoveWindow(hWnd, 10, 10, 500, 500, false);
                        }
                    }
                })).Start();
            }

            return true;
        }

        private void StartForcingFixedStartLocation()
        {
            forcerThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    while (true)
                    {
                        Win32.EnumWindows(MoveIfNotInSetCallback, 0);
                        handles = refreshedHandles;
                        refreshedHandles = new HashSet<IntPtr>();
                    }
                }
                catch (ThreadAbortException e) { }
            }));

            forcerThread.Start();
        }

        private bool InitialSeedCallback(IntPtr hWnd, int lParam)
        {
            handles.Add(hWnd);

            return true;
        }
    }
}
