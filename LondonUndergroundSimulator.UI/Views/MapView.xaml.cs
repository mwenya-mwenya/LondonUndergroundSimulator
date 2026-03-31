using LondonUndergroundSimulator.Colours;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.Rendering;
using LondonUndergroundSimulator.ViewModels;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Windows.UI;

namespace LondonUndergroundSimulator.Views
{
    public sealed partial class MapView : Page
    {

        private readonly MapRenderer _mapRenderer = new();
        private readonly TrainRenderer _trainRenderer;
        private readonly MapTransformService _transformService = new();
        private readonly TimetableService _timetableService;

        float colStation = 150f;
        float colTrainId = 120f;
        float colCountdown = 100f;
        float colStatus = 200f;


        public MainViewModel ViewModel => DataContext as MainViewModel;

        public MapView()
        {
            InitializeComponent();
            //DataContext = new MainViewModel();
            _trainRenderer = new TrainRenderer();
            MapCanvas.PointerMoved += MapCanvas_PointerMoved;
            
            CompositionTarget.Rendering += (s, e) => MapCanvas.Invalidate();
        }

        private void MapCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(MapCanvas).Position;
            ViewModel.MouseX = pt.X;
            ViewModel.MouseY = pt.Y;
        }

        private void SpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ViewModel?.Simulation != null)
            {
                float newSpeed = (float)e.NewValue;
                ViewModel.Simulation.TimeScale = newSpeed;

                // Update status text
                if (newSpeed == 0)
                    SpeedStatusText.Text = "Stopped";
                else
                    SpeedStatusText.Text = $"{newSpeed:0.0}x";
            }
        }

        private void MapCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {

            if (ViewModel == null)
                return;

            // Update simulation ONCE per frame
            ViewModel.Simulation.Update(1f / 120f);

            ViewModel.TimetableEntries = new List<TimetableEntry>(ViewModel.Timetable.BuildTimetable());

            _transformService.Compute(ViewModel, (float)sender.ActualWidth, (float)sender.ActualHeight);

            var ds = args.DrawingSession;
            ds.Transform = Matrix3x2.Identity;

            _mapRenderer.Draw(ds, ViewModel, _transformService);


            foreach (var train in ViewModel.TrainsModel)
            {
                _trainRenderer.Draw(ds, train.Train, _transformService.Transform);

            }

            _trainRenderer.DrawTime(ds,new Vector2(100,20), ViewModel.Simulation.GetFormattedTime());

            float y = 500;
            var format = new CanvasTextFormat
            {
                FontSize = 14,
                FontFamily = "Segoe UI",
                HorizontalAlignment = CanvasHorizontalAlignment.Left,
                VerticalAlignment = CanvasVerticalAlignment.Top
            };

            float headerY = y - 20;
            float x = 20;
            ds.FillRectangle(x, headerY - 2, colStation + colTrainId + colCountdown + colStatus, 20, Colors.White);
            ds.DrawText("Next Station", x, headerY, Colors.Black, format);
            x += colStation;

            ds.DrawText("Train", x, headerY, Colors.Black, format);
            x += colTrainId;

            ds.DrawText("Due", x, headerY, Colors.Black, format);
            x += colCountdown;

            ds.DrawText("Status", x, headerY, Colors.Black, format);


            foreach (var entry in ViewModel.TimetableEntries)
            {
                x = 20;
                ds.FillRectangle(x, y - 2, colStation + colTrainId + colCountdown, 20, Colors.Black);

                
                // Get the train so we can access its line color
                
                var lineColor = ColorExtensions.FromHex(entry.Color);

                // Column 1 — Station name
                ds.DrawText(entry.CurrentStation, x, y, lineColor, format);
                x += colStation;

                // Column 2 — Train ID
                ds.DrawText(entry.TrainId.ToString(), x, y, lineColor, format);
                x += colTrainId;

                // Column 3 — Countdown
                ds.DrawText(entry.GetCountdown(ViewModel.Simulation.Clock), x, y, lineColor, format);
                x += colCountdown;

                // Column 4 — Status
                string statusText = entry.GetStatus().Text;
                string textBackgroundColor = entry.GetStatus().Color;
                
                ds.FillRectangle(x, y, colStatus, 20, ColorExtensions.FromHex(textBackgroundColor));
                ds.DrawText(statusText, x, y, Colors.Black, format);
                x += colStatus;

                y += 20;
            }
            ds.Transform = Matrix3x2.Identity;


        }


    }
}