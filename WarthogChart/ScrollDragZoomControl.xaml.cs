using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WarthogChart
{
    public partial class ScrollDragZoomControl : UserControl
    {
        public static readonly DependencyProperty ZoomLevelProperty = DependencyProperty.Register(
            "ZoomLevel",
            typeof(int),
            typeof(ScrollDragZoomControl),
            new FrameworkPropertyMetadata(
                 1,
                 FrameworkPropertyMetadataOptions.AffectsRender,
                 new PropertyChangedCallback(OnPanelPositionChanged),
                 null
                 )
            );

        public int ZoomLevel
        {
            get { return (int)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        private static void OnPanelPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue) return;

            ScrollDragZoomControl b = d as ScrollDragZoomControl;

            if (b == null) return;

            b.UpdateZoom();
        }

        private ScaleTransform scaleTransform;
        private ScrollViewer scrollViewer;
        private Grid grid;

        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        public ScrollDragZoomControl()
        {
            InitializeComponent();
        }

        private void scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            scaleTransform = (ScaleTransform)Template.FindName("scaleTransform", this);
            scrollViewer = (ScrollViewer)Template.FindName("scrollViewer", this);
            grid = (Grid)Template.FindName("grid", this);

            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.MouseRightButtonUp += OnMouseButtonUp;
            scrollViewer.PreviewMouseRightButtonUp += OnMouseButtonUp;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
            scrollViewer.PreviewMouseRightButtonDown += OnMouseButtonDown;
            scrollViewer.MouseMove += OnMouseMove;
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }

        void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(grid);

            if (e.Delta > 0) ZoomLevel++;
            else if (e.Delta < 0) ZoomLevel--;

            ZoomLevel = ZoomLevel < 1 ? 1 : ZoomLevel;

            scaleTransform.ScaleX = ZoomLevel;
            scaleTransform.ScaleY = ZoomLevel;

            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, grid);

            e.Handled = true;
        }

        public void UpdateZoom()
        {
            ZoomLevel = ZoomLevel < 1 ? 1 : ZoomLevel;

            scaleTransform.ScaleX = ZoomLevel;
            scaleTransform.ScaleY = ZoomLevel;

            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, grid);
        }

        void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, grid);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(grid);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / grid.Width;
                    double multiplicatorY = e.ExtentHeight / grid.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
    }
}