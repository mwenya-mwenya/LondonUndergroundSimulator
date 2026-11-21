using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using System.Diagnostics;
using LondonUndergroundSimulator.Infrastructure.Interfaces;
using LondonUndergroundSimulator.Infrastructure.Services;

namespace LondonUndergroundSimulator.Engine.Logging
{
    public class LogMetrics
    {
        private readonly string _path;

        public LogMetrics(IFilePathService filePathService)
        {
            _path = filePathService.GetUserSimulationLogFilePath();

            string directoryPath = Path.GetDirectoryName(_path);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            Debug.WriteLine($"LogMetrics initialized with path: {_path}");
            if (!File.Exists(_path))
            {
               
                File.WriteAllText(_path,
                    "DayOfWeek,Hour,Minute,TrainId,Station,LineName,StationType," +
                    "WaitingPassengers,CurrentPassengers,MaxCapacity,IsDwelling,IsDelayed\n");
            }
        }

        public void LogSnapshot(List<Train> trains, SimulationClock clock)
        {
            try
            {
                using var writer = File.AppendText(_path);

                foreach (var train in trains)
                {
                    if (!train.Line.IsServiceActive)
                    {
                        continue;
                    }
                    var station = train.Line.Stations.DistinctBy(s => s.Name).ToList()[train.CurrentStationIndex];

                    writer.WriteLine($"{clock.DayOfWeek}," +
                                     $"{clock.Hour}," +
                                     $"{clock.Minute}," +
                                     $"{train.Id}," +
                                     $"{station.Name}," +
                                     $"{train.Line.Name}," +
                                     $"{station.Type}," +
                                     $"{station.WaitingPassengers}," +
                                     $"{train.CurrentPassengers}," +
                                     $"{train.MaxCapacity}," +
                                     $"{train.IsDwelling}," +
                                     $"{train.IsDelayed}");
                }
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"[LogMetrics] Skipped log write (file busy): {ex.Message}");
            }
        }

    }
}
