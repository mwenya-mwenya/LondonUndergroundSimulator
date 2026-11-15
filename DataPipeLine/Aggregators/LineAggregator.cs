using DataPipeLine.Interfaces;
using DataPipeLine.Models;
using LondonUndergroundSimulator.Engine.Models;

namespace DataPipeLine.Aggregators
{
    public class LineAggregator : IAggregator<LineSnapshot>
    {
        public IEnumerable<LineSnapshot> Aggregate(IEnumerable<LogRecord> records)
        {
            // Group by line + 5‑minute window
            var grouped = records
                .GroupBy(r => new
                {
                    r.LineName,
                    Window = r.TimeMinutes / 5
                });

            var snapshots = new List<LineSnapshot>();

            foreach (var g in grouped)
            {
                // Extract basic operational metrics
                double avgWaiting = g.Average(r => r.WaitingPassengers);
                double maxWaiting = g.Max(r => r.WaitingPassengers);
                double avgLoadFactor = g.Average(r => (double)r.CurrentPassengers / r.MaxCapacity);
                int trainCount = g.Select(r => r.TrainId).Distinct().Count();

                // Extract time-of-day values (from first record in window)
                var first = g.First();
                int day = first.DayOfWeek;
                int hour = first.Hour;
                int minute = first.Minute;

                // Compute trend features (difference from previous window)
                double deltaWaiting = 0;
                double deltaLoad = 0;

                // Find previous window for trend calculation
                int prevWindow = g.Key.Window - 1;
                var prevGroup = records
                    .Where(r => r.LineName == g.Key.LineName &&
                                (r.TimeMinutes / 5) == prevWindow);

                if (prevGroup.Any())
                {
                    deltaWaiting = avgWaiting - prevGroup.Average(r => r.WaitingPassengers);
                    deltaLoad = avgLoadFactor - prevGroup.Average(r => (double)r.CurrentPassengers / r.MaxCapacity);
                }

                // Expected demand multiplier (logged by simulator)
                double expectedDemand = first.ExpectedDemandMultiplier;
                var recommended = ComputeRecommendedFrequency(g);
                snapshots.Add(new LineSnapshot
                {
                    LineName = g.Key.LineName,
                    TimeMinutes = g.Key.Window * 5,

                    // Operational metrics
                    AvgWaiting = avgWaiting,
                    MaxWaiting = maxWaiting,
                    AvgLoadFactor = avgLoadFactor,
                    CurrentTrainCount = trainCount,

                    // Raw time-of-day
                    DayOfWeek = day,
                    Hour = hour,
                    Minute = minute,

                    // Cyclical time encoding
                    HourSin = first.HourSin,
                    HourCos = first.HourCos,
                    MinuteSin = first.MinuteSin,
                    MinuteCos = first.MinuteCos,
                    DaySin = first.DaySin,
                    DayCos = first.DayCos,

                    // Peak/off‑peak flags
                    IsMorningPeak = first.IsMorningPeak,
                    IsLunchPeak = first.IsLunchPeak,
                    IsEveningPeak = first.IsEveningPeak,
                    IsWeekend = first.IsWeekend,

                    // Trend features
                    DeltaWaiting = deltaWaiting,
                    DeltaLoadFactor = deltaLoad,

                    // Simulator-derived demand multiplier
                    ExpectedDemandMultiplier = expectedDemand,
                    RecommendedFrequency = recommended
                });
            }

            return snapshots;
        }

        private int ComputeRecommendedFrequency(IEnumerable<LogRecord> window)
        {
            double avgWaiting = window.Average(r => r.WaitingPassengers);
            double avgLoad = window.Average(r => (double)r.CurrentPassengers / r.MaxCapacity);
            int currentTrains = window.Select(r => r.TrainId).Distinct().Count();

            // Simple rule-based logic
            if (avgLoad > 0.85 || avgWaiting > 15)
                return currentTrains + 1;   // Increase frequency
            if (avgLoad < 0.40 && avgWaiting < 5)
                return Math.Max(1, currentTrains - 1); // Reduce frequency

            return currentTrains; // Keep frequency
        }

    }
}
