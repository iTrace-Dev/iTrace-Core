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

        public static GazeHandler Instance { get { return Singleton.Value ; } }

        private GazeHandler()
        {
        }
    }
}