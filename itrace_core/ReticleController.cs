﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTrace_Core
{
    class ReticleController
    {
        private Reticle reticle;
        private bool shown;
        
        public ReticleController()
        {
            reticle = new Reticle();
            GazeHandler.Instance.OnGazeDataRecieved += RecieveGazeData;
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

        private void RecieveGazeData(object sender, GazeDataRecievedEventArgs e)
        {
            reticle.UpdateReticle(e.RecievedGazeData.X, e.RecievedGazeData.Y);
        }
    }
}
