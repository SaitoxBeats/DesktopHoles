using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DecktopHoles.Services.Mouse;

namespace DecktopHoles
{
    public partial class SelectionOverlayWindow : Window
    {
        private readonly IMouseInputService _mouseInputService;
        private Point _dragStart;
        private bool _isDragging;

        public SelectionOverlayWindow(IMouseInputService mouseInputService)
        {
            _mouseInputService = mouseInputService ?? throw new ArgumentNullException(nameof(mouseInputService));
            InitializeComponent();
            Loaded += OnLoaded;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseMove += OnMouseMove;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            KeyDown += OnKeyDown;
        }

        public SelectionResult? SelectedRegion { get; private set; }

        public event EventHandler<SelectionResult>? SelectionConfirmed;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var virtualBounds = _mouseInputService.GetVirtualScreenBounds();
            Left = virtualBounds.Left;
            Top = virtualBounds.Top;
            Width = virtualBounds.Width;
            Height = virtualBounds.Height;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(OverlayCanvas);
            _isDragging = true;
            SelectionRectangle.Visibility = Visibility.Visible;
            SelectionRectangle.Width = 0;
            SelectionRectangle.Height = 0;
            Canvas.SetLeft(SelectionRectangle, _dragStart.X);
            Canvas.SetTop(SelectionRectangle, _dragStart.Y);
            CaptureMouse();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            var current = e.GetPosition(OverlayCanvas);
            var rect = NormalizeRect(_dragStart, current);
            Canvas.SetLeft(SelectionRectangle, rect.Left);
            Canvas.SetTop(SelectionRectangle, rect.Top);
            SelectionRectangle.Width = rect.Width;
            SelectionRectangle.Height = rect.Height;
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            ReleaseMouseCapture();
            _isDragging = false;

            var endPoint = e.GetPosition(OverlayCanvas);
            var selectionRect = NormalizeRect(_dragStart, endPoint);
            if (selectionRect.Width < 1 || selectionRect.Height < 1)
            {
                SelectionRectangle.Visibility = Visibility.Collapsed;
                return;
            }

            var screenSelection = _mouseInputService.TranslateToScreen(selectionRect, new Point(Left, Top));
            SelectedRegion = _mouseInputService.BuildSelectionResult(screenSelection);
            SelectionConfirmed?.Invoke(this, SelectedRegion);
            DialogResult = true;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        }

        private static Rect NormalizeRect(Point start, Point end)
        {
            var x = Math.Min(start.X, end.X);
            var y = Math.Min(start.Y, end.Y);
            var width = Math.Abs(start.X - end.X);
            var height = Math.Abs(start.Y - end.Y);
            return new Rect(x, y, width, height);
        }
    }

    public sealed class SelectionResult
    {
        public SelectionResult(Rect screenRect, Rect monitorRect, string monitorDeviceName)
        {
            ScreenRect = screenRect;
            MonitorRect = monitorRect;
            MonitorDeviceName = monitorDeviceName;
        }

        public Rect ScreenRect { get; }

        public Rect MonitorRect { get; }

        public string MonitorDeviceName { get; }
    }
}
