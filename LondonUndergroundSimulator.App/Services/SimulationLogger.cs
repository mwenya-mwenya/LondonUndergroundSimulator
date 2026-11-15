using LondonUndergroundSimulator.Engine.Models;
using System.Text;

namespace LondonUndergroundSimulator.HostServices.Logging
{
    public class SimulationLogger
    {
        private readonly string _path;

        public SimulationLogger()
        {
            var folder = Path.Combine(Environment.CurrentDirectory, "data");
            Directory.CreateDirectory(folder);

            _path = Path.Combine(folder, "simulation_log.csv");

            if (!File.Exists(_path))
            {
                File.WriteAllText(_path,
                    "Timestamp,TrainId,LineId,StationIndex,PositionX,PositionY,CurrentPassengers,IsDwelling,IsDelayed\n");
            }
        }

        public void LogTrain(Train train)
        {
            var stationIndex = train.CurrentStationIndex;
            var station = train.Line.Stations[stationIndex];

            var line = string.Join(",",
                DateTime.UtcNow.ToString("o"),
                train.Id,
                train.Line.Name,
                stationIndex,
                station.X,
                station.Y,
                train.CurrentPassengers,
                train.IsDwelling,
                train.IsDelayed
            );

            File.AppendAllText(_path, line + "\n");
        }
    }
}
