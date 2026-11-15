using LondonUndergroundSimulator.Infrastructure.Interfaces;
using LondonUndergroundSimulator.Infrastructure.Services;
using MLInferenceEngine.Services;
using System.Diagnostics;

public class PythonInferenceLauncher
{
    private readonly FrequencyInferenceClient _client;
    private Process _process;

    private readonly IFilePathService _paths;

    public PythonInferenceLauncher(
        FrequencyInferenceClient client,
        IFilePathService paths)
    {
        _client = client;
        _paths = paths;
    }

    public async Task Start()
    {
        string dir = _paths.GetPythonInferenceServicePath();
        string getTrainingOutputFile = _paths.GetDefaultTrainingOutputFile("frequency_model.txt");

        // Service already running?
        if (await _client.PingAsync())
        {
            Debug.WriteLine(
            "Python inference service already running.");
            return;
        }

        if (_process != null && !_process.HasExited)
            return; // already running

        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"-m app.server \"{getTrainingOutputFile}\"",
            WorkingDirectory = dir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        _process = new Process();
        _process.StartInfo = psi;




        _process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                Debug.WriteLine($"PYTHON: {e.Data}");
        };

        _process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                Debug.WriteLine($"ERROR: {e.Data}");
        };

        _process.Start();

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
    }
    public async Task Stop()
    {
        if (_process == null || _process.HasExited)
            return;

        Debug.WriteLine("Requesting Python server shutdown…");

        try
        {
            await _client.ShutdownAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Shutdown RPC failed — killing process. Details: {ex.Message}");
        }

        try
        {
            // Give Python 2 seconds to exit gracefully using cancellation token
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(2000));
            await _process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Python did not exit within timeout — forcing kill.");
            // Kills the process and all child processes created by Python
            _process.Kill(entireProcessTree: true);
        }
        finally
        {
            _process.Dispose();
            _process = null;
            Debug.WriteLine($"Python has been fully closed");
        }
    }


    public async Task<bool> IsRunningAsync()
{
    try
    {
        return await _client.PingAsync();
    }
    catch
    {
        return false;
    }
}
}
