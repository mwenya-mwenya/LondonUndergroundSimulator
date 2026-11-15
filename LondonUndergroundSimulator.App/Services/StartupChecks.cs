using LondonUndergroundSimulator.Infrastructure;
using LondonUndergroundSimulator.Infrastructure.Interfaces;
using LondonUndergroundSimulator.Infrastructure.Services;
using LondonUndergroundSimulator.HostServices.Models;
using LondonUndergroundSimulator.Training;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace LondonUndergroundSimulator.HostServices
{
    public class StartupChecks

    {
        private readonly IFilePathService _paths;

        public StartupChecks(IFilePathService filePathService)
        {
            _paths = filePathService;
        }

        private const long MinimumLogSizeBytes = 5000; // ~5 KB of data
        private const int MaxModelAgeDays = 7;         // retrain weekly

        public IEnumerable<StartupCheckResult> RunChecks()
        {
            
            var results = new List<StartupCheckResult>();
            // Check if the log file exists and has sufficient data
            var logFilePath = _paths.GetDefaultCSVFilePath();
            if (!File.Exists(logFilePath))
            {
                results.Add(new StartupCheckResult
                {
                    Name = "Log File Check",
                    Passed = false,
                    Message = $"Log file not found at {logFilePath}."
                });
            }
            else
            {
                var logFileInfo = new FileInfo(logFilePath);
                if (logFileInfo.Length < MinimumLogSizeBytes)
                {
                    results.Add(new StartupCheckResult
                    {
                        Name = "Log File Check",
                        Passed = false,
                        Message = $"Log file is too small ({logFileInfo.Length} bytes)."
                    });
                }
                else
                {
                    results.Add(new StartupCheckResult
                    {
                        Name = "Log File Check",
                        Passed = true,
                        Message = "Log file is present and has sufficient data."
                    });
                }
            }
            // Check if the model exists and is up-to-date
            var modelStatusPath = _paths.GetDefaultModelStatusFile();
            if (!File.Exists(modelStatusPath))
            {
                results.Add(new StartupCheckResult
                {
                    Name = "Model Status Check",
                    Passed = false,
                    Message = $"Model status file not found at {modelStatusPath}."
                });
            }
            else
            {
                var modelStatusJson = File.ReadAllText(modelStatusPath);

                try
                {
                    var modelStatus = JsonSerializer.Deserialize<ModelStatus>(modelStatusJson);
                    if (modelStatus == null || (DateTime.Now - modelStatus.LastTrained).TotalDays > MaxModelAgeDays)
                    {
                        results.Add(new StartupCheckResult
                        {
                            Name = "Model Status Check",
                            Passed = false,
                            Message = $"Model is outdated. Last trained on {modelStatus?.LastTrained.ToShortDateString() ?? "unknown"}."
                        });
                    }
                    else
                    {
                        results.Add(new StartupCheckResult
                        {
                            Name = "Model Status Check",
                            Passed = true,
                            Message = $"Model is up-to-date. Last trained on {modelStatus.LastTrained.ToShortDateString()}."
                        });
                    }
                }
                catch (JsonException)
                {
                    results.Add(new StartupCheckResult
                    {
                        Name = "Model Status Check",
                        Passed = false,
                        Message = $"Failed to parse model status file at {modelStatusPath}."
                    });
                }
                var modelPath = _paths.GetDefaultTrainingOutputFile("frequency_model.txt");

                results.Add(new StartupCheckResult
                {
                    Name = "Model File Check",
                    Passed = File.Exists(modelPath),
                    Message = File.Exists(modelPath)
                              ? "frequency_model.txt found."
                              : $"frequency_model.txt missing at {modelPath}"
                });

                var pythonInferenceService = _paths.GetPythonInferenceServicePath();

                results.Add(new StartupCheckResult
                {
                    Name = "Python Inference Directory Check",
                    Passed = Directory.Exists(pythonInferenceService),
                    Message = Directory.Exists(pythonInferenceService)
                              ? "Python inference directory found."
                              : $"Python inference directory missing at {pythonInferenceService}"
                });

                var aggregatedDataPath = _paths.GetDefaultAggregatedDataDirectory();

                results.Add(new StartupCheckResult
                {
                    Name = "Aggregated Data File Check",
                    Passed = Directory.Exists(aggregatedDataPath),
                    Message = Directory.Exists(aggregatedDataPath)
                              ? "Aggregated data directory found."
                              : $"Aggregated data directory missing at {aggregatedDataPath}"
                });

            }
            return results;
        }

        public class ModelStatus
        {
            public DateTime LastTrained { get; set; }
        }

        public int GetProgressPercentage(StartupCheckResult[] results)
        {
            if (!results.Any())
                return 0;

            var passed = results.Count(x => x.Passed);

            return (int)(100.0 * passed / results.Count());
        }

        public string GetOverallStatus(StartupCheckResult[] results)
        {
            var failures = results
                .Where(x => !x.Passed)
                .ToList();

            if (!failures.Any())
                return "System Ready";

            return failures.First().Message;
        }
    }
}