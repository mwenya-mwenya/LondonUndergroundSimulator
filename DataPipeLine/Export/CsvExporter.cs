using DataPipeLine.Interfaces;
using LondonUndergroundSimulator.Infrastructure;
using LondonUndergroundSimulator.Infrastructure.Interfaces;
using System.Diagnostics;

namespace DataPipeLine.Export
{
    public class CsvExporter : IExporter
    {
        private readonly IFilePathService _path;

        public CsvExporter(IFilePathService filePathService)
        {
            _path = filePathService;
        }
  
        public void Export<T>(string path, IEnumerable<T> rows)
        {
            Debug.WriteLine($"[Export] Starting export process for type: {typeof(T).Name}");

            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Debug.WriteLine($"[Export] Directory does not exist. Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }

            Debug.WriteLine($"[Export] Exporting aggregated data to: {path}");

            using var writer = new StreamWriter(path);

            var props = typeof(T).GetProperties();
            Debug.WriteLine($"[Export] Found {props.Length} properties to export.");

            var headers = string.Join(",", props.Select(p => p.Name));
            writer.WriteLine(headers);
            Debug.WriteLine($"[Export] Wrote CSV Headers: {headers}");

            int rowCount = 0;
            foreach (var row in rows)
            {
                rowCount++;
                var values = props.Select(p => p.GetValue(row)?.ToString() ?? "");
                writer.WriteLine(string.Join(",", values));
            }

            Debug.WriteLine($"[Export] Finished exporting. Total rows written: {rowCount}");
        }

    }
}
