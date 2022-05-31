/********************************************************************************************************************************************************
* @file EventReplayer.cs
*
* @Copyright (C) 2022 i-trace.org
*
* This file is part of iTrace Infrastructure http://www.i-trace.org/.
* iTrace Infrastructure is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* iTrace Infrastructure is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with iTrace Infrastructure. If not, see <https://www.gnu.org/licenses/>.
********************************************************************************************************************************************************/

using System;
using System.Threading;
using System.Windows.Forms;

namespace iTrace_Core
{
    public abstract class EventReplayer
    {
        protected ComputerEventReader eventReader;
        protected Thread replayerThread;

        public bool IsReplayInProgress { get; protected set; }

        public event EventHandler<EventArgs> OnReplayFinished;

        protected virtual void Replay()
        {
            while (!eventReader.Finished())
            {
                ComputerEvent computerEvent = eventReader.ReadEvent();
                computerEvent.Replay();
                computerEvent.Pause();
                //SocketServer.Instance().SendToClients(computerEvent.Serialize());
                //WebSocketServer.Instance().SendToClients(computerEvent.Serialize());
            }
            ReplayDone();
        }

        protected void ReplayDone()
        {
            IsReplayInProgress = false;
            OnReplayFinished?.Invoke(this, new EventArgs());
        }

        public void StartReplay()
        {
            if (!IsReplayInProgress)
            {
                IsReplayInProgress = true;

                replayerThread = new Thread(Replay);
                replayerThread.Start();
            }
        }

        public void StopReplay()
        {
            eventReader.Close();

            if (replayerThread != null)
            {
                SocketServer.Instance().CancelWait();
                replayerThread.Abort();
            }
        }
    }

    public class FixedPauseEventReplayer : EventReplayer
    {
        public FixedPauseEventReplayer(string filename, int pause)
        {
            eventReader = new ComputerEventReader(filename, new FixedPauseEventFactoryMap(pause));
        }
    }

    public class BidirectionalCommunicationEventReplayer : EventReplayer
    {
        public BidirectionalCommunicationEventReplayer(string filename, int pause)
        {
            eventReader = new ComputerEventReader(filename, new BidirectionalCommunicationFactoryMap(pause));
        }
    }

    public class ProportionalEventReplayer : EventReplayer
    {
        private ComputerEvent nextEvent;
        private ComputerEvent currentEvent;

        public ProportionalEventReplayer(string filename, int scale)
        {
            eventReader = new ComputerEventReader(filename, new ProportionalFactoryMap(scale));
        }

        // Warning: A minimum of 3 events is required to be present in the file 
        // being read from. Otherwise, this function will crash. 
        protected override void Replay()
        {
            currentEvent = eventReader.ReadEvent();
            nextEvent = eventReader.ReadEvent();

            do
            {
                if (currentEvent.PauseStrategy is EmptyPause)
                {
                    currentEvent.Replay();
                    currentEvent.Pause();
                }
                else
                {
                    ProportionalLengthPause strategy = currentEvent.PauseStrategy as ProportionalLengthPause;
                    strategy.NextEventTime = nextEvent.EventTime;

                    currentEvent.Replay();
                    currentEvent.Pause();
                }

                currentEvent = nextEvent;
                nextEvent = eventReader.ReadEvent();
                
            } while (!eventReader.Finished());

            currentEvent.Replay();
            currentEvent.Pause();
            nextEvent.Replay();

            ReplayDone();
        }
    }

}
