using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    public sealed class GazeHandler
    {
        public event EventHandler<GazeDataReceivedEventArgs> OnGazeDataReceived;

        private static readonly Lazy<GazeHandler> Singleton =
            new Lazy<GazeHandler>(() => new GazeHandler());

        private System.Collections.Concurrent.BlockingCollection<GazeData> GazeQueue;

        public static GazeHandler Instance { get { return Singleton.Value ; } }

        public void StartHandler()
        {
            GazeQueue = new System.Collections.Concurrent.BlockingCollection<GazeData>(new System.Collections.Concurrent.ConcurrentQueue<GazeData>());
            new System.Threading.Thread(() =>
            {
                DequeueGaze();
            }).Start();
        }

        public void EnqueueGaze(GazeData gd)
        {
            Console.WriteLine("ENQUEUE!");
            GazeQueue.Add(gd);
        }

        private void DequeueGaze()
        {
            GazeData gd = GazeQueue.Take();
            while (!gd.IsEmpty())
            {
                if (OnGazeDataReceived != null)
                {
                    OnGazeDataReceived(this, new GazeDataReceivedEventArgs(gd));
                }

                Console.WriteLine("DEQUEUE!");
                Console.WriteLine(gd.Output());
                gd = GazeQueue.Take();
            }
            Console.WriteLine("DONE!");
        }
    }

    public class GazeDataReceivedEventArgs : EventArgs
    {
        public GazeData ReceivedGazeData { get; private set; }

        public GazeDataReceivedEventArgs(GazeData gazeData)
        {
            ReceivedGazeData = gazeData;
        }
    }
}