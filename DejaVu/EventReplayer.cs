using DejaVuLib;
using System;
using System.Threading;
using System.Windows.Forms;

namespace DejaVu
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
            }
            ReplayDone();
        }

        protected void ReplayDone()
        {
            // TODO: Figure out way to unminimize window when replay is done
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
                if (currentEvent.PauseStrategy is FixedLengthPause)
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
