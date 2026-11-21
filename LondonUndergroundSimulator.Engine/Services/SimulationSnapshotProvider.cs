//using System.Linq;
//using MLInferenceEngine.Abstractions.Interfaces;
//using MLInferenceEngine.Abstractions.Models;
//using LondonUndergroundSimulator.Engine.Models;
//using System.Diagnostics;
//using static LondonUndergroundSimulator.Engine.Logging.LogBuffer;

//namespace LondonUndergroundSimulator.Engine.Services
//{
//    public class SimulationSnapshotProvider : ISimulationSnapshotProvider
//    {
//        private readonly LiveTelemetryBuffer _buffer;
//        private readonly IAggregator<LineSnapshot> _aggregator;

//        public SimulationSnapshotProvider(
//            LiveTelemetryBuffer buffer,
//            IAggregator<LineSnapshot> aggregator)
//        {
//            _buffer = buffer;
//            _aggregator = aggregator;
//        }

//        public SimulationSnapshot CaptureSnapshot()
//        {
//            throw new NotImplementedException();
//        }

//        public IReadOnlyList<SimulationSnapshot> CaptureSnapshots()
//        {
//            var recentRecords = _buffer.GetRecentRecords();
//            if (recentRecords.Count == 0)
//                return new List<SimulationSnapshot>();

//            // Reuse the exact same LineAggregator used during training
//            var allLineSnapshots = _aggregator.Aggregate(recentRecords);

//            // Pick the most recent 5-minute window for each line
//            var latestPerLine = allLineSnapshots
//                .GroupBy(s => s.LineName)
//                .Select(g => g.OrderByDescending(s => s.TimeMinutes).First())
//                .ToList();

//            // Map LineSnapshot -> SimulationSnapshot
//            return latestPerLine.Select(s => new SimulationSnapshot
//            {
//                LineId = s.LineName,
//                AvgWaiting = s.AvgWaiting,
//                MaxWaiting = s.MaxWaiting,
//                AvgLoadFactor = s.AvgLoadFactor,
//                CurrentTrainCount = s.CurrentTrainCount,

//                DayOfWeek = s.DayOfWeek,
//                Hour = s.Hour,
//                Minute = s.Minute,
//                TimeMinutes = s.TimeMinutes,

//                HourSin = s.HourSin,
//                HourCos = s.HourCos,
//                MinuteSin = s.MinuteSin,
//                MinuteCos = s.MinuteCos,
//                DaySin = s.DaySin,
//                DayCos = s.DayCos,

//                IsMorningPeak = s.IsMorningPeak,
//                IsLunchPeak = s.IsLunchPeak,
//                IsEveningPeak = s.IsEveningPeak,
//                IsWeekend = s.IsWeekend,

//                DeltaWaiting = s.DeltaWaiting,
//                DeltaLoadFactor = s.DeltaLoadFactor,
//                ExpectedDemandMultiplier = s.ExpectedDemandMultiplier
//            }).ToList();
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using MLInferenceEngine.Abstractions.Interfaces;
using MLInferenceEngine.Abstractions.Models;
using LondonUndergroundSimulator.Engine.Services;

namespace LondonUndergroundSimulator.Engine.Services
{
    public class SimulationSnapshotProvider : ISimulationSnapshotProvider
    {
        private readonly SimulationEngine _engine;

        // Stores the previous snapshot per line to compute DeltaWaiting and DeltaLoadFactor
        private readonly Dictionary<string, (double AvgWaiting, double AvgLoadFactor)> _previousMetrics = new();

        public SimulationSnapshotProvider(SimulationEngine engine)
        {
            _engine = engine;
        }

        public IReadOnlyList<SimulationSnapshot> CaptureSnapshots()
        {
            var snapshots = new List<SimulationSnapshot>();

            var clock = _engine.Clock24;
            int day = clock.DayOfWeek;
            int hour = clock.Hour;
            int minute = clock.Minute;
            int timeMinutes = hour * 60 + minute;

            double hourSin = Math.Sin(2 * Math.PI * hour / 24.0);
            double hourCos = Math.Cos(2 * Math.PI * hour / 24.0);
            double minuteSin = Math.Sin(2 * Math.PI * minute / 60.0);
            double minuteCos = Math.Cos(2 * Math.PI * minute / 60.0);
            double daySin = Math.Sin(2 * Math.PI * day / 7.0);
            double dayCos = Math.Cos(2 * Math.PI * day / 7.0);

            bool isMorningPeak = hour >= 7 && hour < 9;
            bool isLunchPeak = hour >= 12 && hour < 14;
            bool isEveningPeak = hour >= 17 && hour < 19;
            bool isWeekend = day == 6 || day == 0;
            double expectedDemand = _engine.GetTimeOfDayMultiplier(clock);

            // Snapshot _trains once on the main thread for thread safety
            var allTrains = _engine.trainsIReadonly.ToList();

            foreach (var line in _engine.Lines)
            {
                if (!line.IsServiceActive)
                    continue;

                var lineTrains = allTrains
                    .Where(t => t.Line == line)
                    .DistinctBy(t => t.Id)
                    .ToList();

                var stations = line.Stations
                    .DistinctBy(s => s.Name)
                    .ToList();

                double avgWaiting = stations.Count == 0
                    ? 0
                    : stations.Average(s => s.WaitingPassengers);

                double maxWaiting = stations.Count == 0
                    ? 0
                    : stations.Max(s => s.WaitingPassengers);

                double avgLoadFactor = lineTrains.Count == 0
                    ? 0
                    : lineTrains.Average(t => t.MaxCapacity > 0 ? (double)t.CurrentPassengers / t.MaxCapacity : 0);

                // Compute trend features matching LineAggregator
                double deltaWaiting = 0;
                double deltaLoad = 0;

                if (_previousMetrics.TryGetValue(line.Name, out var prev))
                {
                    deltaWaiting = avgWaiting - prev.AvgWaiting;
                    deltaLoad = avgLoadFactor - prev.AvgLoadFactor;
                }

                _previousMetrics[line.Name] = (avgWaiting, avgLoadFactor);

                snapshots.Add(new SimulationSnapshot
                {
                    LineName = line.Name,
                    AvgWaiting = avgWaiting,
                    MaxWaiting = maxWaiting,
                    AvgLoadFactor = avgLoadFactor,
                    CurrentTrainCount = allTrains.Count,

                    DayOfWeek = day,
                    Hour = hour,
                    Minute = minute,
                    TimeMinutes = timeMinutes,

                    HourSin = hourSin,
                    HourCos = hourCos,
                    MinuteSin = minuteSin,
                    MinuteCos = minuteCos,
                    DaySin = daySin,
                    DayCos = dayCos,

                    IsMorningPeak = isMorningPeak,
                    IsLunchPeak = isLunchPeak,
                    IsEveningPeak = isEveningPeak,
                    IsWeekend = isWeekend,

                    DeltaWaiting = deltaWaiting,
                    DeltaLoadFactor = deltaLoad,
                    ExpectedDemandMultiplier = expectedDemand
                });
            }

            return snapshots;
        }
    }
}
