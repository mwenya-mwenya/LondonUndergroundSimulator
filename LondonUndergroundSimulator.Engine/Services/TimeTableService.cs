using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LondonUndergroundSimulator.Engine.Services
{
    /// <summary>
    /// Builds immutable timetable entries based on the current simulation state.
    /// Contains NO simulation logic, NO movement logic, NO delay logic.
    /// Pure snapshot builder.
    /// </summary>
    public class TimetableService
    {
        private readonly SimulationEngine _engine;
        private readonly Dictionary<string, float> _lingerTimers = new();
        private readonly Dictionary<string, bool> _wasBusy = new();


        public TimetableService(SimulationEngine engine)
        {
            _engine = engine;
        }

        /// <summary>
        /// Builds a fresh timetable snapshot for all trains.
        /// </summary>
        public List<TimetableEntry> BuildTimetable()
        {
            var entries = new List<TimetableEntry>();

            foreach (var train in _engine._trains)
            {
                var entry = BuildEntry(train);
                entries.Add(entry);
            }

            return entries;
        }

        private TimetableEntry BuildEntry(Train train)
        {
            var line = train.Line;

            UpdateServiceStatus(_engine.Clock24);

            string id = train.Id;

            int waiting = line.Stations[train.CurrentStationIndex].WaitingPassengers;

            // Ensure dictionary entries exist
            if (!_lingerTimers.ContainsKey(id))
                _lingerTimers[id] = 0f;

            if (!_wasBusy.ContainsKey(id))
                _wasBusy[id] = false;

            // LINGER LOGIC
            if (waiting >= 30)
            {
                _wasBusy[id] = true;
                _lingerTimers[id] = 2.0f; // linger for 2 seconds
            }
            else
            {
                if (_wasBusy[id])
                {
                    _lingerTimers[id] -= 1f / 60f; // approx per frame at 60fps
                    if (_lingerTimers[id] <= 0)
                        _wasBusy[id] = false;
                }
            }

            bool show = _wasBusy[id] || waiting >= 30;

            if (line.IsServiceActive == true)
            {
                var entry = new TimetableEntry
                {
                    TrainId = train.Id,
                    LineName = line.Name,
                    CurrentStation = line.Stations[train.CurrentStationIndex].Name,
                    Color = line.ColorHex,
                    ScheduledArrival = train.ScheduledArrivalTime,
                    ExpectedArrival = train.ExpectedArrivalTime,
                    DelayDuration = TimeSpan.FromSeconds(train.DelayDuration),
                    DelayStartTime = train.DelayStartTime,
                    DelayEndTime = train.DelayStartTime + TimeSpan.FromSeconds(train.DelayDuration),
                    IsDelayed = train.IsDelayed,
                    IsDwelling = train.IsDwelling,
                    DelayReasons = new List<string>(train.DelayReasons),
                    CurrentPassengers = train.CurrentPassengers,
                    CurrentStationWaitingPassengers = line.Stations[train.CurrentStationIndex].WaitingPassengers,
                    ShouldDisplay = show
                    
                };

               
                return entry;
            } else
            {
                return new TimetableEntry
                {
                    TrainId = "N/A",
                    LineName = line.Name,
                    ServiceStatus = false,
                    ShouldDisplay = false
                };
            }

            
        }


        public void UpdateServiceStatus(SimulationClock clock)
        {
            foreach (var line in _engine.Lines)
            {
                if (clock.Hour >= 5 || clock.Hour < 1)
                {
                    line.IsServiceActive = true;
                }
                else
                {
                    line.IsServiceActive = false;
                }
            }


        }
    }
}