/********************************************************************************************************************************************************
* @file IPauseStrategy.cs
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
