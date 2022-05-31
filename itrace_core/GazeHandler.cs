/********************************************************************************************************************************************************
* @file GazeHandler.cs
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
using System.Threading.Tasks;

namespace iTrace_Core
{
    public sealed class GazeHandler
    {
        public event EventHandler<GazeDataReceivedEventArgs> OnGazeDataReceived;

        private static readonly Lazy<GazeHandler> Singleton =
            new Lazy<GazeHandler>(() => new GazeHandler());

        private System.Collections.Concurrent.BlockingCollection<GazeData> GazeQueue;

        public static GazeHandler Instance { get { return Singleton.Value ; } }

        public void StartHandler()
        {
            GazeQueue = new System.Collections.Concurrent.BlockingCollection<GazeData>(new System.Collections.Concurrent.ConcurrentQueue<GazeData>());
            new System.Threading.Thread(() =>
            {
                DequeueGaze();
            }).Start();
        }

        public void EnqueueGaze(GazeData gd)
        {
            GazeQueue.Add(gd);
        }

        private void DequeueGaze()
        {
            GazeData gd = GazeQueue.Take();
            while (gd != null)
            {
                if (OnGazeDataReceived != null)
                {
                    OnGazeDataReceived(this, new GazeDataReceivedEventArgs(gd));
                }
                //Console.WriteLine(gd.Output());
                gd = GazeQueue.Take();
            }
            Console.WriteLine("Queue Thread Done!");
        }
    }

    public class GazeDataReceivedEventArgs : EventArgs
    {
        public GazeData ReceivedGazeData { get; private set; }

        public GazeDataReceivedEventArgs(GazeData gazeData)
        {
            ReceivedGazeData = gazeData;
        }
    }
}