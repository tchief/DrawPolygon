using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace DrawPolygon
{
    public class VisualHost : FrameworkElement
    {
        private const string VertexNameFormat = "N{0}";

        private readonly VisualCollection _children;
        private readonly List<Point> _points;

        private readonly Pen _edgePen = new Pen(Brushes.Black, 1);
        private readonly Brush _vertexBrush = Brushes.Red;
        private readonly Pen _vertexPen = new Pen(Brushes.Black, 1);
        private readonly int _vertexRadius = 6;

        private DrawingContext _currentLineContext;
        private DrawingVisual _currentLineVisual;

        public VisualHost()
        {
            _children = new VisualCollection(this);
            _points = new List<Point>();
            _edgePen.Freeze();
            _vertexPen.Freeze();
        }

        protected bool IsClosed { get; private set; }
        protected bool IsEdgeDrawing { get { return _currentLineContext != null; } }
        protected override int VisualChildrenCount { get { return _children.Count; } }
        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

        public void DrawVertex(Point center)
        {
            if (IsClosed) return;
            if (IsEdgeDrawing)
                FinishActiveEdge();

            var visual = CreateNewDrawing();
            using (var dc = visual.RenderOpen())
                dc.DrawEllipse(_vertexBrush, _vertexPen, center, _vertexRadius, _vertexRadius);

            _points.Add(center);
        }

        public void DrawEdge(Point start, Point finish)
        {
            if (IsClosed) return;
            if (IsEdgeDrawing)
                FinishActiveEdge();
            else
                _currentLineVisual = CreateNewDrawing();

            _currentLineContext = _currentLineVisual.RenderOpen();
            _currentLineContext.DrawLine(_edgePen, start, finish);

            _currentLineContext.DrawEllipse(_vertexBrush, _vertexPen, start, _vertexRadius, _vertexRadius);
        }

        public void ClosePolygon()
        {
            if (IsClosed || !_points.Any()) return;
            DrawEdge(_points.Last(), _points.First());
            DrawVertex(_points.First());
            IsClosed = true;
            DrawVertexNames();
        }

        public void Clear()
        {
            _children.Clear();
            _points.Clear();
            IsClosed = false;
        }

        private void FinishActiveEdge()
        {
            _currentLineContext.Close();
            _currentLineContext = null;
        }

        private DrawingVisual CreateNewDrawing()
        {
            var visual = new DrawingVisual();
            _children.Add(visual);
            return visual;
        }

        private void DrawVertexNames()
        {
            var interval = Observable.Generate(
                    0L,
                    i => i < _points.Count - 1,
                    i => i + 1,
                    i => (int)i,
                    i => TimeSpan.FromSeconds(0.3))
                .ObserveOnDispatcher()
                .Subscribe(i =>
                {
                    var visual = CreateNewDrawing();
                    using (var dc = visual.RenderOpen())
                        DrawVertexName(dc, _points[i], i + 1);
                });
        }

        private void DrawVertexName(DrawingContext dc, Point vertexCenter, int index)
        {
            var nextVertexName = new FormattedText(string.Format(VertexNameFormat, index), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Verdana"), 12, Brushes.Red);
            var textCenter = new Point(vertexCenter.X, vertexCenter.Y);
            textCenter.Offset(_vertexRadius, _vertexRadius);
            dc.DrawText(nextVertexName, textCenter);
        }
    }
}
