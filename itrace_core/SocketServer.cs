using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class SocketServer
    {
        public void Start()
        {
            GazeHandler.Instance.OnGazeDataRecieved += RecieveGazeData;            
        }

        private void RecieveGazeData(object sender, GazeDataRecievedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
