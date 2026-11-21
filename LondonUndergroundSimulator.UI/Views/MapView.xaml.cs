using Windows.Storage;
using LondonUndergroundSimulator.Rendering;
using LondonUndergroundSimulator.Services;
using LondonUndergroundSimulator.ViewModels;
using LondonUndergroundSimulator.HostServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Xml;
using Windows.UI;
using static System.Collections.Specialized.BitVector32;
using MLInferenceEngine.Abstractions.Interfaces;

namespace LondonUndergroundSimulator.Views
{
    public sealed partial class MapView : Page
    {
        private readonly MapRenderer _mapRenderer;
        private readonly TrainRenderer _trainRenderer;
        private readonly MapTransformService _transformService;
        private readonly DrawUITimetable _drawUITimetable;
        private readonly CreateMLConsole _createMLConsole;

        public ITrainingService _trainingService;

        private readonly StartupChecks _startupChecks;
        private readonly SimulationHost _simulationHost;

        //private readonly PythonInferenceLauncher _pythonInferenceLauncher;
        public MainViewModel ViewModel => DataContext as MainViewModel;

        public MapView()
        {
            InitializeComponent();

            Loaded += Page_Loaded;

                _mapRenderer = App.Services.GetRequiredService<MapRenderer>();

                _trainRenderer = App.Services.GetRequiredService<TrainRenderer>();

                _transformService = App.Services.GetRequiredService<MapTransformService>();

                _drawUITimetable = App.Services.GetRequiredService<DrawUITimetable>();

                _createMLConsole = App.Services.GetRequiredService<CreateMLConsole>();

                _startupChecks = App.Services.GetRequiredService<StartupChecks>();

                _trainingService = App.Services.GetRequiredService<ITrainingService>();

                _simulationHost = App.Services.GetRequiredService<SimulationHost>();

            MapCanvas.PointerMoved += MapCanvas_PointerMoved;
            CompositionTarget.Rendering += (s, e) => MapCanvas.Invalidate();

        }

        private void MapCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pt = e.GetCurrentPoint(MapCanvas).Position;
            ViewModel.MouseX = pt.X;
            ViewModel.MouseY = pt.Y;
        }

        private void MapCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            
            if (ViewModel == null || !ViewModel.StartupComplete)
                return;            

            ViewModel.Simulation.Update(1f / 120f);

            _transformService.Compute(ViewModel, (float)sender.ActualWidth, (float)sender.ActualHeight);

            var ds = args.DrawingSession;
            ds.Transform = Matrix3x2.Identity;

            _mapRenderer.Draw(ds, ViewModel, _transformService);

            foreach (var train in ViewModel.Map.Trains)
                _trainRenderer.Draw(ds, train.Train, _transformService.Transform);
            _trainRenderer.DrawTime(ds, new Vector2(100, 50), $"Time: {ViewModel.Simulation.Clock24}");
            _drawUITimetable.DrawTimetable(ds, ViewModel, (float)sender.ActualWidth, (float)sender.ActualHeight);
            _createMLConsole.DrawMLConsole(ds, ViewModel, (float)sender.ActualWidth, (float)sender.ActualHeight);

            ds.Transform = Matrix3x2.Identity;
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

        private async void RetrainButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Mark as retraining (this updates IsRetrainingText and disables the button implicitly)
                ViewModel.IsRetraining = true;

                // 2. Explicitly tell the UI that CanRetrainModel needs to recalculate
                ViewModel.PropertyChanged += (s, args) => { }; // Fallback representation
                                                               // Since CanRetrainModel depends on IsRetraining, we force a refresh here:
                ViewModel.GetType().GetMethod("OnRefreshTrainingStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(ViewModel, null);

                // 3. Run the processing
                await _simulationHost.RetrainModelAsync(userInitiated: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UI] Error during model retraining: {ex.Message}");
            }
            finally
            {
                // 4. Reset the flag when complete, which allows button clicks again
                ViewModel.IsRetraining = false;

                // 5. Force a final recalculation check
                // We can invoke your existing private helper method on the VM to do this perfectly:
                typeof(MainViewModel)
                    .GetMethod("OnRefreshTrainingStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(ViewModel, null);
            }
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.RunStartupChecksAsync(
                _startupChecks
            );

            if (!ViewModel.StartupChecks.All(x => x.Passed))
            {
                Debug.WriteLine("Startup checks failed.");
                return;
            }

            Debug.WriteLine("Startup checks passed.");
        }

    }

}