using DataPipeLine.Interfaces;
using LondonUndergroundSimulator.Infrastructure.Interfaces;
using System.Diagnostics; // Added for Debug

namespace DataPipeLine
{
    public class Pipeline
    {
        private readonly ILogReader _reader;
        private readonly IEnumerable<IAggregator> _aggregators;
        private readonly IExporter _exporter;
        private readonly IFilePathService _paths;

        public Pipeline(
            ILogReader reader,
            IEnumerable<IAggregator> aggregators,
            IExporter exporter,
            IFilePathService paths)
        {
            _reader = reader;
            _aggregators = aggregators;
            _exporter = exporter;
            _paths = paths;
        }

        public void Run(string logPath)
        {
            Debug.WriteLine($"\n[Pipeline] === Starting Pipeline Run ===");
            Debug.WriteLine($"[Pipeline] Source Log Input Path: {logPath}");

            if (!File.Exists(logPath))
            {
                Debug.WriteLine($"[Pipeline] ERROR: Source log file does not exist at {logPath}");
                return;
            }

            var records = _reader.Read(logPath).ToList();
            Debug.WriteLine($"[Pipeline] Successfully read {records.Count} raw records from log.");

            foreach (var aggregator in _aggregators)
            {
                var aggregatorName = aggregator.GetType().Name;
                Debug.WriteLine($"[Pipeline] Processing data with: {aggregatorName}");

                var result = ((dynamic)aggregator).Aggregate(records);

                // Turn "StationAggregator" into "station_features.csv"
                var finalFileName = aggregatorName.Replace("Aggregator", "_features.csv").ToLowerInvariant();

                // Direct the file into your User Aggregated Data Directory
                var targetDir = _paths.GetUserAggregatedDataDirectory();
                var outputPath = Path.Combine(targetDir, finalFileName);

                // Execute the file creation
                _exporter.Export(outputPath, result);

                // Verify the file was physically created right after export finishes
                if (File.Exists(outputPath))
                {
                    var fileInfo = new FileInfo(outputPath);
                    Debug.WriteLine($"[Pipeline] SUCCESS: Aggregated file created safely!");
                    Debug.WriteLine($"           -> File Size: {fileInfo.Length} bytes");
                }
                else
                {
                    Debug.WriteLine($"[Pipeline] CRITICAL: Exporter finished, but file is MISSING from disk at {outputPath}");
                }
            }

            Debug.WriteLine($"[Pipeline] === Finished Pipeline Run ===\n");
        }

    }
}
