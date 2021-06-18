using DejaVuLib;
using System.IO;

namespace DejaVu
{
    public class ComputerEventWriter
    {
        string filename;
        StreamWriter streamWriter;

        public ComputerEventWriter(string filename)
        {
            this.filename = filename;
            streamWriter = new StreamWriter(filename);
        }
        private object locker = new object();
        public void Write(ComputerEvent computerEvent)
        {
            lock(locker)
            {
                streamWriter.Write(computerEvent.Serialize());
            }
        }

        public void Close()
        {
            streamWriter.Flush();
            streamWriter.Close();
        }
    }
}
