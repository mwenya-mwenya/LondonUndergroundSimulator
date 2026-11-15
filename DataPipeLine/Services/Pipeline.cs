using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Infrastructure.Interfaces;
using System.Text.Json;

namespace DataPipeline
{
    public class JsonDataPipeline : IDataPipeline
    {
        private readonly IFilePathService _paths;

        public JsonDataPipeline(IFilePathService paths)
        {
            _paths = paths;
        }

        public List<Line> LoadLines()
        {
            var path = _paths.GetDefaultDataFile("lines.json");
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Line>>(json)!;
        }

        public List<Train> LoadTrains(List<Line> lines)
        {
            var path = _paths.GetDefaultDataFile("trains.json");
            var json = File.ReadAllText(path);
            var trains = JsonSerializer.Deserialize<List<Train>>(json)!;

            foreach (var train in trains)
            {
                train.Line = lines.FirstOrDefault(l => l.Trains.Any(t => t.Id == train.Id));
            }

            return trains;
        }
    }
}
