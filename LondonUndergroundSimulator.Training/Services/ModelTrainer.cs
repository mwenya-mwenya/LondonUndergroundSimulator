using LondonUndergroundSimulator.Infrastructure.Interfaces;
using System.Diagnostics;
using System.Text.Json;

namespace LondonUndergroundSimulator.Training
{
    public class ModelTrainer
    {
        private readonly IFilePathService _path;
        public ModelTrainer(IFilePathService filePathService)
        {
            _path = filePathService;
        }

        
        public void Train(bool userInitiated)
        {
            Debug.WriteLine("Training ML model...");

            var dataDir = _path.GetDefaultDataDirectory();


            var getAggregatedDataDirectory = _path.GetDefaultAggregatedDataDirectory();

            var getTrainingOutputDirectory = _path.GetDefaultTrainingOutputDirectory();

            if (userInitiated)
            {               

                Debug.WriteLine($"Has user initiated training? {userInitiated}");

                    getAggregatedDataDirectory = _path.GetUserAggregatedDataDirectory();
                    getTrainingOutputDirectory = _path.GetUserDataDirectory();                
            }


            Debug.WriteLine($"Exists: {Directory.Exists(getAggregatedDataDirectory)}, Physical path: {Path.GetDirectoryName(getAggregatedDataDirectory)}");

            foreach (var file in Directory.GetFiles(getAggregatedDataDirectory))
            {
                Debug.WriteLine($"C# sees: {file}");
            }

            

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "python",
                ArgumentList = {"train_model.py", getAggregatedDataDirectory, getTrainingOutputDirectory},
                WorkingDirectory = dataDir,
                UseShellExecute = false,                
                RedirectStandardOutput = true,                
                RedirectStandardError = true
            });

            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    Debug.WriteLine($"PYTHON: {e.Data}");
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    Debug.WriteLine($"ERROR: {e.Data}");
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();


            process?.WaitForExit();

            // Write model status
            var status = new ModelStatus
            {
                LastTrained = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(status, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_path.GetDefaultModelStatusFile(), json);

            Debug.WriteLine("Model training complete.");
        }

 
    }

    public class ModelStatus
    {
        public DateTime LastTrained { get; set; }
    }
}
