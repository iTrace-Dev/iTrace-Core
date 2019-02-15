namespace iTrace_Core
{
    class ReticleController
    {
        private Reticle reticle;
        private bool shown;

        public ReticleController()
        {
            reticle = new Reticle();
            GazeHandler.Instance.OnGazeDataReceived += ReceiveGazeData;
            shown = false;
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
            reticle.UpdateReticle(e.ReceivedGazeData.X, e.ReceivedGazeData.Y);
        }
    }
}
