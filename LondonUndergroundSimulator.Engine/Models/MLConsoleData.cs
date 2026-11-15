using MLInferenceEngine.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Models
{
    public class MLConsoleData
    {
        public SimulationSnapshot LatestSnapshot { get; set; }

        public int LatestRecommendation { get; set; }

        public int LastAppliedFrequency { get; set; }

        public List<MLConsoleLogEntry> Log { get; set; } = new();

        // New stats
        public int SimulationLogRows { get; set; }

        public string LastTimeModelUpdated { get; set; }

        public long SimulationLogSizeBytes { get; set; }

        public string SimulationLogSizeDisplay =>
            $"{SimulationLogSizeBytes / 1024.0:F2} KB";

        public int TrainingReadinessPercent { get; set; }

        public int TrainingProgressPercent { get; set; }
    }

}
