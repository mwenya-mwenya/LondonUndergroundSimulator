using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LondonUndergroundSimulator.ViewModels
{
    public class MapViewModel
    {
        private readonly SimulationEngine _engine;

        public IReadOnlyList<Line> Lines { get; }
        public ObservableCollection<TrainViewModel> Trains { get; } = new ObservableCollection<TrainViewModel>();

        private MainViewModel _mainViewModel;

        public MapViewModel(IReadOnlyList<Line> lines, SimulationEngine engine)
        {
            Lines = lines;
            _engine = engine;

            // Subscribe to frequency change events
            //_engine.FrequencyChanged += OnFrequencyChanged;
            _engine.FleetChanged += RefreshTrains;

            // Initial load
            RefreshTrains();
        }


        private void RefreshTrains()
        {
            Trains.Clear();

            foreach (var train in _engine._trains)
                Trains.Add(new TrainViewModel(train));
        }

    }
}
