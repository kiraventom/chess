using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Common;
using GUI.Utils;
using GUI.ViewModels;
using Logic;
using SkiaSharp.Views.Desktop;

namespace GUI;

public partial class MainWindow
{
    private const float Fps = 15;

    private readonly ChangeTracker _changeTracker;

    private float BoardSide =>
        (float) (MainView.ActualWidth < MainView.ActualHeight ? MainView.ActualWidth : MainView.ActualHeight);

    private float FieldSide => BoardSide / 8;

    private ResizeEndHelper _resizeEndHelper;
    private ViewModel ViewModel => (ViewModel) DataContext;

    private Position _hoveredPosition;

    public MainWindow(ChangeTracker changeTracker)
    {
        InitializeComponent();

        _changeTracker = changeTracker;

        MainView.PaintSurface += OnPaint;
        MainView.MouseMove += OnMouseMove;
        MainView.MouseDown += OnMouseDown;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _resizeEndHelper = new ResizeEndHelper(MainView);
        _resizeEndHelper.ResizeEnd += ViewModel.OnForceRedraw;

        var timer = new DispatcherTimer(DispatcherPriority.Render, Dispatcher.CurrentDispatcher)
        {
            Interval = TimeSpan.FromSeconds(1.0 / Fps)
        };

        timer.Tick += OnTick;

        ViewModel.Init();
        timer.Start();
    }

    private void OnTick(object sender, EventArgs e)
    {
        _resizeEndHelper.OnTick();
        MainView.InvalidateVisual();
    }

    private async void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_hoveredPosition.IsValid)
            await ViewModel.OnClick(_hoveredPosition);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var point = e.GetPosition(MainView);
        var col = (int) Math.Floor(point.X / FieldSide) + 1;
        var row = 8 - (int) Math.Floor(point.Y / FieldSide);

        _hoveredPosition = new Position(row, col);
    }

    private void OnPaint(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;

        var changes = _changeTracker.GetAll();

        foreach (var change in changes)
        foreach (var layer in change.Layers)
            layer.Paint(canvas, FieldSide);
    }
}