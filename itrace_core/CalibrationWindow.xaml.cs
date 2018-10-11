using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace iTrace_Core
{
    public partial class CalibrationWindow : Window
    {
        public event EventHandler<CalibrationPointReachedEventArgs> OnCalibrationPointReached;

        public CalibrationWindow()
        {
            InitializeComponent();
        }
    }

    public class CalibrationPointReachedEventArgs : EventArgs
    {
        public Point CalibrationPoint { get; private set; }

        public CalibrationPointReachedEventArgs(Point point)
        {
            CalibrationPoint = point;
        }
    }
}
