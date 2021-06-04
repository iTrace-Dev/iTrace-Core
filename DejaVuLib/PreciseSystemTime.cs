using System.Runtime.InteropServices;

namespace DejaVuLib
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
