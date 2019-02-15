using System.Windows;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace iTrace_Core
{
    public partial class EyeStatusWindow : Window
    {
        string registeredLeftEyeName = "leftEyeCircle";
        string registeredRightEyeName = "rightEyeCircle";

        EllipseGeometry leftEyeCircle;
        EllipseGeometry rightEyeCircle;

        public EyeStatusWindow()
        {
            InitializeComponent();
        }

        public void UpdateEyePosition(Vector3 leftEyePosition, Vector3 rightEyePosition)
        {

        }
    }
}
