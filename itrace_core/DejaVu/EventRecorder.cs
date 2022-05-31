/********************************************************************************************************************************************************
* @file EventRecorder.cs
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
    public class EventRecorder
    {
        public bool IsRecordInProgress { get; private set; }

        KeyboardHook keyPressListener = new KeyboardHook();
        MouseHook mouseListener = new MouseHook();
        //CoreClient gazeListener = new CoreClient();
        ComputerEventWriter eventWriter;

        //constructor
        public EventRecorder(ComputerEventWriter writer)
        {
            eventWriter = writer;

            mouseListener.OnMouseEvent += ComputerEventListener;
            mouseListener.HookMouse();

            keyPressListener.OnKeyboardEvent += ComputerEventListener;
            keyPressListener.HookKeyboard();

            GazeHandler.Instance.OnGazeDataReceived += WriteCoreMessage;
            //gazeListener.OnCoreMessage += ComputerEventListener;
        }

        private void WriteCoreMessage(object sender, GazeDataReceivedEventArgs e)
        {
            CoreMessage msg = new CoreMessage();

            msg.DeserializeFrom(e.ReceivedGazeData.Serialize());
            if (IsRecordInProgress)
                eventWriter.Write(msg);


        }

        private void ComputerEventListener(object sender, ComputerEvent e)
        {
            if (IsRecordInProgress)
                eventWriter.Write(e);
        }

        public void StartRecording()
        {
            IsRecordInProgress = true;
        }

        public void StopRecording()
        {
            IsRecordInProgress = false;
        }

        /*
        public void ConnectToCore()
        {
            gazeListener.Connect();
        }
        */

        public void Dispose()
        {
            keyPressListener.Dispose();
            mouseListener.Dispose();
            eventWriter.Close();
        }
    }
}
