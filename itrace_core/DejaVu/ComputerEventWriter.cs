using System.IO;

namespace iTrace_Core
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
