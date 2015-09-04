using System.Windows;
using System.Windows.Input;

namespace DrawPolygon
{
    public partial class MainWindow : Window
    {
        private Point? _currentPoint;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _currentPoint = e.GetPosition(Host);
            Host.DrawVertex(_currentPoint.Value);
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Host.ClosePolygon();
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                _currentPoint = null;
                Host.Clear();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_currentPoint == null)
                return;

            Host.DrawEdge(_currentPoint.Value, e.GetPosition(Host));
        }

        private void Canvas_KeyUp(object sender, KeyEventArgs e)
        {
            
        }
    }
}
