using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.HostServices;
using LondonUndergroundSimulator.HostServices.Interfaces;
using LondonUndergroundSimulator.HostServices.Models;
using LondonUndergroundSimulator.ViewModels;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MapViewModel Map { get; set; }
        public SimulationEngine Simulation { get; set; }
        public TimetableService Timetable { get; set; }
        public List<TimetableEntry> TimetableEntries { get; set; }
        public MLConsoleData MLConsole => Simulation.MLConsoleData;
        private readonly IInferenceStatusService _status;
        public MainViewModel(IInferenceStatusService status)
        {
            _status = status;
            _ = UpdateMlStatusAsync();

        }

        public string TimeScaleText => $"{Simulation.TimeScale}.0x";

        private bool _isMlConnected;
        public bool IsMlConnected
        {
            get => _isMlConnected;
            private set
            {
                _isMlConnected = value;
                OnPropertyChanged();
            }
        }

        private async Task UpdateMlStatusAsync()
        {
            while (true)
            {
                 var status = await _status.IsRunningAsync();

                IsMlConnected = status;

                await Task.Delay(4000); // check every 10 seconds
            }
        }

        public ObservableCollection<StartupCheckResult> StartupChecks { get; } = new();

        private int _startupProgress;
        public string StartupProgressText => $"{StartupProgress}% Complete";

        public Visibility StartupVisibility => StartupComplete
                                                ? Visibility.Collapsed
                                                : Visibility.Visible;
        public int StartupProgress
        {
            get => _startupProgress;
            set
            {
                _startupProgress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartupProgressText));
                
            }
        }

        private string _startupFailureReason;

        public string StartupFailureReason
        {
            get => _startupFailureReason;
            set
            {
                _startupFailureReason = value;
                OnPropertyChanged();
            }
        }

        private bool _startupComplete;

        public bool StartupComplete
        {
            get => _startupComplete;
            set
            {
                _startupComplete = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartupVisibility));
            }
        }

        private string _checksPassedText;

        public string ChecksPassedText
        {
            get => _checksPassedText;
            set
            {
                _checksPassedText = value;
                OnPropertyChanged();
            }
        }

        public float PassengerDemandMultiplier
        {
            get => Simulation?.PassengerDemandMultiplier ?? 1f;
            set
            {
                if (Simulation != null)
                {
                    Simulation.PassengerDemandMultiplier = value;
                    OnPropertyChanged();
                }
            }
        }

        
        
        private bool _isRetraining;

        public bool IsRetraining
        {
            get => _isRetraining;
            set
            {
                _isRetraining = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRetrainingText));
                OnPropertyChanged(nameof(CanRetrainModel));

            }
        }

        public string IsRetrainingText =>  IsRetraining ? "Retraining...": "Retrain Model";

        public bool CanRetrainModel => !IsRetraining &&  (MLConsole?.SimulationLogRows ?? 0) >= 5000;

        //private void OnRefreshTrainingStatus()
        //{
        //    OnPropertyChanged(nameof(CanRetrainModel));
        //}



        // -----------------------------
        // Mouse + Hover UI properties
        // -----------------------------
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
        public void AttachEngine(SimulationEngine engine)
        {
            Simulation = engine;
            Timetable = new TimetableService(engine);
            TimetableEntries = Timetable.BuildTimetable();
            Map = new MapViewModel(engine.Lines, engine);
            Simulation.RetrainingStatusChanged += () => OnPropertyChanged(nameof(CanRetrainModel));
            //Simulation.RetrainingStatusChanged += OnRefreshTrainingStatus;

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public async Task RunStartupChecksAsync(HostServices.StartupChecks startupChecks)
        {
            
            StartupChecks.Clear();

            var results = startupChecks.RunChecks().ToList();
            var failedCheck = results.FirstOrDefault(x => !x.Passed);

            if (results.Count == 0)
            {
                StartupProgress = 100;
                StartupComplete = true;
                return;
            }

            for (int i = 0; i < results.Count; i++)
            {
                StartupChecks.Add(results[i]);               

                StartupProgress =
                    (int)(((i + 1.0) / results.Count) * 100);

                await Task.Delay(2500); // nice visual effect
            }

           var allPassed = results.All(x => x.Passed);

            if (allPassed)
            {
                ChecksPassedText = "All checks passed. Starting the simulation...";
                await Task.Delay(4000);
                StartupComplete = true;
            }
            else

                if (failedCheck != null)
            {
                StartupFailureReason = failedCheck.Message;
            }
        }


    }
}
