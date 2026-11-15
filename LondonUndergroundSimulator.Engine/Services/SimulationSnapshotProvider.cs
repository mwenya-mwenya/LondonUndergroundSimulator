using System.Linq;
using MLInferenceEngine.Abstractions.Interfaces;
using MLInferenceEngine.Abstractions.Models;
using LondonUndergroundSimulator.Engine.Models;

namespace LondonUndergroundSimulator.Engine.Services
{
    public class SimulationSnapshotProvider : ISimulationSnapshotProvider
    {
        private readonly SimulationEngine _engine;

        public SimulationSnapshotProvider(SimulationEngine engine)
        {
            _engine = engine;
        }


        public SimulationSnapshot CaptureSnapshot()
        {
            var trains = _engine._trains;
            var stations = trains.SelectMany(t => t.Line.Stations).ToList();

            // -----------------------------
            // Operational metrics
            // -----------------------------
            double avgWaiting = stations.Count == 0
                ? 0
                : stations.Average(s => s.WaitingPassengers);

            double maxWaiting = stations.Count == 0
                ? 0
                : stations.Max(s => s.WaitingPassengers);

            double avgLoadFactor = trains.Count == 0
                ? 0
                : trains.Average(t => (double)t.CurrentPassengers / t.MaxCapacity);

            int trainCount = trains.Count;

            // -----------------------------
            // Time-of-day (from Clock24)
            // -----------------------------
            var clock = _engine.Clock24;

            int day = clock.DayOfWeek;
            int hour = clock.Hour;
            int minute = clock.Minute;

            int timeMinutes = hour * 60 + minute;

            // -----------------------------
            // Cyclical time encoding
            // -----------------------------
            double hourSin = Math.Sin(2 * Math.PI * hour / 24.0);
            double hourCos = Math.Cos(2 * Math.PI * hour / 24.0);

            double minuteSin = Math.Sin(2 * Math.PI * minute / 60.0);
            double minuteCos = Math.Cos(2 * Math.PI * minute / 60.0);

            double daySin = Math.Sin(2 * Math.PI * day / 7.0);
            double dayCos = Math.Cos(2 * Math.PI * day / 7.0);

            // -----------------------------
            // Peak/off-peak flags
            // -----------------------------
            bool isMorningPeak = hour >= 7 && hour < 9;
            bool isLunchPeak = hour >= 12 && hour < 14;
            bool isEveningPeak = hour >= 17 && hour < 19;
            bool isWeekend = day == 6 || day == 0;

            // -----------------------------
            // Expected demand multiplier
            // (from your simulator’s passenger model)
            // -----------------------------
            double expectedDemand = _engine.GetTimeOfDayMultiplier(clock);

            // -----------------------------
            // Trend features (optional)
            // These require storing previous snapshot values.
            // If you want, I can add a trend buffer here.
            // -----------------------------
            double deltaWaiting = 0;
            double deltaLoad = 0;

            // -----------------------------
            // Build snapshot
            // -----------------------------
            return new SimulationSnapshot
            {
                // Operational metrics
                AvgWaiting = avgWaiting,
                MaxWaiting = maxWaiting,
                AvgLoadFactor = avgLoadFactor,
                CurrentTrainCount = trainCount,

                // Raw time-of-day
                DayOfWeek = day,
                Hour = hour,
                Minute = minute,
                TimeMinutes = timeMinutes,

                // Cyclical encoding
                HourSin = hourSin,
                HourCos = hourCos,
                MinuteSin = minuteSin,
                MinuteCos = minuteCos,
                DaySin = daySin,
                DayCos = dayCos,

                // Peak flags
                IsMorningPeak = isMorningPeak,
                IsLunchPeak = isLunchPeak,
                IsEveningPeak = isEveningPeak,
                IsWeekend = isWeekend,

                // Trend features
                DeltaWaiting = deltaWaiting,
                DeltaLoadFactor = deltaLoad,

                // Simulator-derived demand multiplier
                ExpectedDemandMultiplier = expectedDemand
            };
        }
    }
}
