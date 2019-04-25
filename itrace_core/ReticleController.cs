using System.Threading;

namespace iTrace_Core
{
    class ReticleController
    {
        private Reticle reticle;
        private bool shown;
        private bool closed;
        Mutex mutex = new Mutex();

        public ReticleController()
        {
            reticle = new Reticle();
            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;
            shown = false;
            closed = false;
        }

        public void ShowReticle()
        {
            reticle.ToDraw(true);
            shown = true;
        }

        public void HideReticle()
        {
            reticle.ToDraw(false);
            shown = false;
        }

        public bool IsShown()
        {
            return shown;
        }

        private void ReceiveGazeData(object sender, GazeDataReceivedEventArgs e)
        {
            mutex.WaitOne();

            if (e.ReceivedGazeData.IsValid() && !closed && e.ReceivedGazeData.X.HasValue && e.ReceivedGazeData.Y.HasValue)
            {
                reticle.UpdateReticle(e.ReceivedGazeData.X.Value, e.ReceivedGazeData.Y.Value);
            }

            mutex.ReleaseMutex();
        }

        public void Close()
        {
            closed = true;
            reticle.CompleteEvents();

            mutex.WaitOne();

            reticle.CompleteEvents();
            reticle.Close();

            mutex.ReleaseMutex();
        }
    }
}
