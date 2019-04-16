using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace iTrace_Core
{
    static class PreciseSystemTime
    {
        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void GetSystemTimePreciseAsFileTime(out long filetime);

        public static long GetTime()
        {
            long time;
            GetSystemTimePreciseAsFileTime(out time);
            return time;
        }
    }
}
