using LondonUndergroundSimulator.Infrastructure.Interfaces;
using System;
using System.Diagnostics;
using System.IO;


namespace LondonUndergroundSimulator.Infrastructure.Services
{
    public class FilePathService : IFilePathService
    {


        public Func<string>? PackagePathResolver { get; set; }

        public string MainDirectory { get; }

        public FilePathService()
        {
            MainDirectory = AppContext.BaseDirectory;
        }

        public string GetUserDataDirectory()
        {
            // 1. If running in WinUI 3, it uses the UI's injected path rule
            if (PackagePathResolver != null)
            {
                return Path.Combine(PackagePathResolver(),"LondonUndergroundSimulator");
            }

            // 2. Otherwise, fall back to standard non-packaged local path behavior
            return GetDefaultDataDirectory();


        }

        public string GetUserDataDirectoryFile(string filename)
        {
             return Path.Combine(PackagePathResolver(), filename);             
        }

        public string GetUserAggregatedDataDirectory()
        {
            return Path.Combine(GetUserDataDirectory(), "AggregatedData");
        }

        public List<string> GetUserAggregatedDataList(string fileName) =>
               Directory.GetFiles(GetUserAggregatedDataDirectory(), fileName).ToList();

        public List<string> GetUserAggregatedDataListUserInitiated(string fileName) =>
               Directory.GetFiles(GetUserDataDirectory(), fileName).ToList();


        public string GetPythonInferenceServicePath()
        {
            return Path.Combine(MainDirectory, "PythonInferenceService");
        }

        public string GetDefaultDataDirectory()
        {
            return Path.Combine(MainDirectory, "Data");
        }

        public string GetDefaultDataFile(string fileName)
        {
            return Path.Combine(GetDefaultDataDirectory(), fileName);
        }

        public string GetDefaultTrainingDataDirectory()
        {
            return Path.Combine(GetDefaultDataDirectory(), "TrainingModels");
        }

        public string GetDefaultTrainingDataDirectoryFile(string fileName)
        {
            return Path.Combine(GetDefaultTrainingDataDirectory(), fileName);
        }

        public string GetDefaultAggregatedDataDirectory()
        {
            return Path.Combine(GetDefaultTrainingDataDirectory(), "AggregatedData");
        }

        public string GetDefaultAggregatedDataDirectoryFile(string filename)
        {
            return Path.Combine(GetDefaultAggregatedDataDirectory(), filename);
        }

        public List<string> GetDefaultDataFileList(string fileName) =>
               Directory.GetFiles(GetDefaultAggregatedDataDirectory(), fileName).ToList();

        public string GetDefaultModelStatusFile()
        {
            return Path.Combine(GetDefaultTrainingDataDirectory(), "model_status.json");
        }

        public string GetDefaultCSVFilePath()
        {
            return Path.Combine(GetDefaultDataDirectory(), "simulation_log.csv");
        }

        public string GetDefaultTrainingModelFilePath()
        {
            return Path.Combine(GetDefaultTrainingDataDirectory(), "TrainingModels", "train_model.py");
        }

        public string GetDefaultTrainingOutputFile(string fileName)
        {
            return Path.Combine(GetDefaultTrainingDataDirectory(), "TrainingOutput", fileName);
        }

        public string GetDefaultTrainingOutputDirectory()
        {
            return Path.Combine(GetDefaultTrainingDataDirectory(), "TrainingOutput");
        }
    }
}
