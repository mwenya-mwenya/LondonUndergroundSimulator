using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LondonUndergroundSimulator.Engine.Data
{
    public class LineTrainConfig
    {
        public int Trains { get; set; }
        public int Carriages { get; set; }
    }

    public static class TrainConfig
    {
        public static Dictionary<string, LineTrainConfig> Lines { get; private set; }

        static TrainConfig()
        {
            var json = File.ReadAllText("Data/train_config.json");
            Lines = JsonSerializer.Deserialize<Dictionary<string, LineTrainConfig>>(json);
        }
    }
}