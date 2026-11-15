using DataPipeLine.Interfaces;
using DataPipeLine.Models;

namespace DataPipeLine.Readers
{
    public class LogReader : ILogReader
    {
        public IEnumerable<LogRecord> Read(string path)
        {
            var lines = File.ReadAllLines(path).Skip(1); // skip header

            foreach (var line in lines)
            {
                var parts = line.Split(','); // your file IS comma-separated

                yield return new LogRecord
                {
                    DayOfWeek = int.Parse(parts[0]),
                    Hour = int.Parse(parts[1]),
                    Minute = int.Parse(parts[2]),

                    TrainId = parts[3],
                    Station = parts[4],
                    LineName = parts[5],
                    StationType = parts[6],

                    WaitingPassengers = int.Parse(parts[7]),
                    CurrentPassengers = int.Parse(parts[8]),
                    MaxCapacity = int.Parse(parts[9]),

                    IsDwelling = parts[10].Equals("true", StringComparison.OrdinalIgnoreCase),
                    IsDelayed = parts[11].Equals("true", StringComparison.OrdinalIgnoreCase)
                };
            }
        }

    }
}
