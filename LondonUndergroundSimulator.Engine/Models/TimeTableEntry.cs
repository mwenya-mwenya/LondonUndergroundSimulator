

namespace LondonUndergroundSimulator.Engine.Models
{
    /// <summary>
    /// Immutable snapshot of a train's timetable state.
    /// Contains NO simulation logic.
    /// Provides pure countdown formatting.
    /// </summary>
    public class TimetableEntry
    {
        public string TrainId { get; init; }
        public string LineName { get; init; }
        public string CurrentStation { get; init; }
        public string Color { get; init; }

        public TimeSpan ScheduledArrival { get; init; }
        public TimeSpan ExpectedArrival { get; init; }

        public TimeSpan DelayDuration { get; init; }
        public TimeSpan DelayStartTime { get; init; }
        public TimeSpan DelayEndTime { get; init; }

        public bool IsDelayed { get; init; }
        public bool IsDwelling { get; init; }

        public record TimetableStatus(string Text, string Color);

        public IReadOnlyList<string> DelayReasons { get; init; }

        public string GetCountdown(TimeSpan currentTime)
        {
            var remaining = ExpectedArrival - currentTime;

            if (IsDwelling)
                return "Loading...";

            if (remaining.TotalSeconds <= 0)
                return "Due";

            if (remaining.TotalMinutes < 1)
                return $"{remaining.Seconds} sec";

            return $"{(int)remaining.TotalMinutes}:{remaining.Seconds:D2} mins";
        }


        public TimetableStatus GetStatus()
        {
            if (IsDwelling)
            {
                return new TimetableStatus(
                    "Arrived",
                     "4682B4"
                );
            }

            if (IsDelayed)
            {
                string reason = DelayReasons.Count > 0
                    ? $"Delayed – {string.Join(", ", DelayReasons)}"
                    : "Delayed";

                return new TimetableStatus(
                    reason,
                     "FFD700"
                );
            }

            return new TimetableStatus(
                "Running",
                 "228B22"
            );
        }
    }
}