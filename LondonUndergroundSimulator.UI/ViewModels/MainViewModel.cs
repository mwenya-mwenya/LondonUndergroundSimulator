using LondonUndergroundSimulator.Engine.Data;
using LondonUndergroundSimulator.Engine.DelayServices;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace LondonUndergroundSimulator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MapViewModel Map { get; set; }
        public ObservableCollection<TrainViewModel> TrainsModel { get; set; }

        public SimulationEngine Simulation { get; }
        public TimetableService Timetable { get; }
        public List<TimetableEntry> TimetableEntries { get; set; }

        private double _mouseX;
        public double MouseX
        {
            get => _mouseX;
            set { _mouseX = value; OnPropertyChanged(nameof(MouseX)); }
        }

        private double _mouseY;
        public double MouseY
        {
            get => _mouseY;
            set { _mouseY = value; OnPropertyChanged(nameof(MouseY)); }
        }

        private Station _hoveredStation;
        public Station HoveredStation
        {
            get => _hoveredStation;
            set { _hoveredStation = value; OnPropertyChanged(nameof(HoveredStation)); }
        }

        public MainViewModel()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Data", "lines.json");
            var lines = MapLoader.Load(path);

            // 1. Build trains
            var trains = Trains.GetTrains();

            // 2. Build services 
            var delayService = new TrainDelayService();
            var failureEngine = new FailureRuleEngine();

            // 3. Add failure rules to the failure engine
            failureEngine.AddRule(new SignalFailureRule());

            // 4. Create SimulationEngine with injected services
            Simulation = new SimulationEngine(
                trains,
                delayService,
                failureEngine
            );

            // 5. Create TimetableService
            Timetable = new TimetableService(Simulation);

            // 6. Build timetable entries
            TimetableEntries = Timetable.BuildTimetable();

            // 7. Wrap trains in view models
            TrainsModel = new ObservableCollection<TrainViewModel>(
                trains.Select(t => new TrainViewModel(t))
            );

            // 8. Create MapViewModel LAST
            Map = new MapViewModel(lines, Simulation);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}