/********************************************************************************************************************************************************
* @file ReticleController.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System.Threading;

namespace iTrace_Core
{
    class ReticleController
    {
        private Reticle reticle;
        private bool shown;
        private bool closed;
        Mutex mutex = new Mutex();

        public ReticleController()
        {
            reticle = new Reticle();
            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;
            shown = false;
            closed = false;
        }

        public void ShowReticle()
        {
            reticle.ToDraw(true);
            shown = true;
        }

        public void HideReticle()
        {
            reticle.ToDraw(false);
            shown = false;
        }

        public bool IsShown()
        {
            return shown;
        }

        private void ReceiveGazeData(object sender, GazeDataReceivedEventArgs e)
        {
            mutex.WaitOne();

            if (e.ReceivedGazeData.IsValid() && !closed && e.ReceivedGazeData.X.HasValue && e.ReceivedGazeData.Y.HasValue)
            {
                reticle.UpdateReticle(e.ReceivedGazeData.X.Value, e.ReceivedGazeData.Y.Value);
            }

            mutex.ReleaseMutex();
        }

        public void Close()
        {
            closed = true;
            reticle.CompleteEvents();

            mutex.WaitOne();

            reticle.CompleteEvents();
            reticle.Close();

            mutex.ReleaseMutex();
        }
    }
}
