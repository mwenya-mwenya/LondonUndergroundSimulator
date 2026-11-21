using DataPipeLine;
using DataPipeLine.Aggregators;
using DataPipeLine.Export;
using DataPipeLine.Readers;
using DataPipeLine.Interfaces;
using LondonUndergroundSimulator.Infrastructure.Services;
using System.Reflection.Metadata;
using DataPipeLine;
using LondonUndergroundSimulator.Infrastructure.Interfaces;
using System.Diagnostics;

namespace LondonUndergroundSimulator.Training.Services
{
    public class TrainingHost
    {
        private readonly Pipeline _pipeline;
        private readonly ModelTrainer _trainer;
        private readonly IFilePathService _paths;

        public TrainingHost(Pipeline pipeline, ModelTrainer trainer, IFilePathService paths)
        {
            _pipeline = pipeline;
            _trainer = trainer;
            _paths = paths;
        }

        public void Run(bool userInitiated)
        {
            Debug.WriteLine($"[TrainingHost] Running training pipeline. User Initiated: {userInitiated}");

            var aggregatedUserDataDirectory = _paths.GetUserAggregatedDataDirectory();

            if (!Directory.Exists(aggregatedUserDataDirectory))
            {
                Debug.WriteLine("[TrainingHost] User data directory missing. Creating directory...");
                Directory.CreateDirectory(aggregatedUserDataDirectory);
            }

            var searchPattern = "*imulation_*.csv";
            var targetFiles = userInitiated ? _paths.GetUserAggregatedDataListUserInitiated(searchPattern).ToList() : _paths.GetUserAggregatedDataList(searchPattern).ToList();

            Debug.WriteLine($"[TrainingHost] Found {targetFiles.Count} simulation files matching pattern '{searchPattern}'.");

            Parallel.ForEach(targetFiles, file =>
            {
                _pipeline.Run(file);
            });
            _trainer.Train(userInitiated);

        }
    }
}
