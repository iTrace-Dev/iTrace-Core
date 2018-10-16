using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    public sealed class GazeHandler
    {
        private static readonly Lazy<GazeHandler> Singleton =
            new Lazy<GazeHandler>(() => new GazeHandler());

        private System.Collections.Concurrent.BlockingCollection<String> GazeQueue;
            
        public static GazeHandler Instance { get { return Singleton.Value ; } }

        private GazeHandler()
        {
            GazeQueue = new System.Collections.Concurrent.BlockingCollection<String>(new System.Collections.Concurrent.ConcurrentQueue<String>());
        }
    }
}