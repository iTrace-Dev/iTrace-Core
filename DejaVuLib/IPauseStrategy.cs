using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DejaVuLib
{
    public interface IPauseStrategy
    {
        void Pause(ComputerEvent toPause);
    }

    public class EmptyPause : IPauseStrategy
    {
        public void Pause(ComputerEvent toPause) { }
    }

    public class WaitForClientPause : IPauseStrategy
    {
        const int timeoutLength = 5000;

        public void Pause (ComputerEvent toPause)
        {
            SocketServer.Instance().WaitUntilClientsAreReady(timeoutLength);
        }
    }

    public class FixedLengthPause : IPauseStrategy
    {
        private int lengthInMilliseconds;

        public FixedLengthPause(int periodInMilliseconds)
        {
            lengthInMilliseconds = periodInMilliseconds;
        }

        public void Pause (ComputerEvent toPause)
        {
            Thread.Sleep(lengthInMilliseconds);
        }
    }
    
    public class ProportionalLengthPause : IPauseStrategy
    {
        public long NextEventTime { get; set; }

        private int scaleProportion;

        private static long HundredNanosecondsToMilliseconds(long hundredNanoseconds)
        {
            return hundredNanoseconds / 10000;
        }

        public ProportionalLengthPause(int scaleProportion)
        {
            this.scaleProportion = scaleProportion;
        }

        public void Pause(ComputerEvent toPause)
        {
            long difference = NextEventTime - toPause.EventTime;

            if (difference < 0)
                return;

            Thread.Sleep(Convert.ToInt32(HundredNanosecondsToMilliseconds(difference)) * scaleProportion);
        }
    }
}
