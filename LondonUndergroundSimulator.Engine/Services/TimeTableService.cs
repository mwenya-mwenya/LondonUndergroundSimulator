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

            foreach (var train in _engine.Trains)
            {
                var entry = BuildEntry(train);
                entries.Add(entry);
            }

            return entries;
        }

        private TimetableEntry BuildEntry(Train train)
        {
            return new TimetableEntry
            {
                TrainId = train.Id,
                LineName = train.Line.Name,
                CurrentStation = train.Line.Stations[train.CurrentStationIndex].Name,
                Color = train.Line.ColorHex,

                ScheduledArrival = train.ScheduledArrivalTime,
                ExpectedArrival = train.ExpectedArrivalTime,

                DelayDuration = TimeSpan.FromSeconds(train.DelayDuration),
                DelayStartTime = train.DelayStartTime,
                DelayEndTime = train.DelayStartTime + TimeSpan.FromSeconds(train.DelayDuration),

                IsDelayed = train.IsDelayed,
                IsDwelling = train.IsDwelling,
                DelayReasons = new List<string>(train.DelayReasons)
            };
        }

        
    }
}