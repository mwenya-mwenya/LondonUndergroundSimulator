using LondonUndergroundSimulator.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Services
{
    public class SimulationLogStats
    {
        private readonly IFilePathService _paths;

        private readonly string _simulationLogFile;

        public SimulationLogStats(IFilePathService paths)
        {
            _paths = paths;
            _simulationLogFile = _paths.GetUserSimulationLogFilePath();
        }

        public int GetRowCount()
        {
            

            if (!File.Exists(_simulationLogFile))
                return 0;

            return File.ReadLines(_simulationLogFile).Skip(1).Count();
        }

        public long GetFileSize()
        {

            if (!File.Exists(_simulationLogFile))
                return 0;

            var size = new FileInfo(_simulationLogFile).Length;

            return size;
        }

        public string GetDateModelLastUpdate()
        {
            var modelStatusPath = _paths.GetDefaultModelStatusFile();
            var modelStatusJson = File.ReadAllText(modelStatusPath);
            var modelStatus = JsonSerializer.Deserialize<ModelStatus>(modelStatusJson);

            string date = modelStatus?.LastTrained.ToLocalTime().ToString("dd/MM/yy HH:mm") ?? "unknown";

            return date;

        }
        public class ModelStatus
        {
            public DateTime LastTrained { get; set; }
        }
    }
}
