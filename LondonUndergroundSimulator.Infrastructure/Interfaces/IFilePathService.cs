using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Infrastructure.Interfaces
{
    public interface IFilePathService
    {
        string GetDefaultDataDirectory();
        string GetDefaultDataFile(string fileName);
        string GetDefaultTrainingDataDirectory();
        string GetDefaultAggregatedDataDirectory();
        string GetDefaultAggregatedDataDirectoryFile(string filename);
        string GetDefaultModelStatusFile();
        string GetDefaultCSVFilePath();
        string GetDefaultTrainingModelFilePath();
        string GetDefaultTrainingOutputDirectory();
        string GetDefaultTrainingOutputFile(string fileName);
        List<string> GetDefaultDataFileList(string fileName);


        Func<string>? PackagePathResolver { get; set; }
        string GetUserDataDirectory();

        public string GetUserDataDirectoryFile(string filename);

        public string GetUserAggregatedDataDirectory();

        public List<string> GetUserAggregatedDataList(string filename);
        string GetUserSimulationLogFilePath() =>
            Path.Combine(
                GetUserDataDirectory(),
                "simulation_log.csv");
        public string GetPythonInferenceServicePath();
    }

}
