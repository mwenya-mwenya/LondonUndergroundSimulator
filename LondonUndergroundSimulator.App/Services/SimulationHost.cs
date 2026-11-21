using DataPipeLine;
using DataPipeLine;
using DataPipeLine.Aggregators;
using DataPipeLine.Export;
using DataPipeLine.Interfaces;
using DataPipeLine.Readers;
using LondonUndergroundSimulator.Engine.DelayServices;
using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Logging;
using LondonUndergroundSimulator.Engine.Models;
using LondonUndergroundSimulator.Engine.Services;
using LondonUndergroundSimulator.HostServices.Interfaces;
using LondonUndergroundSimulator.HostServices.Logging;
using LondonUndergroundSimulator.HostServices.Services;
using LondonUndergroundSimulator.Infrastructure.Interfaces;
using LondonUndergroundSimulator.Infrastructure.Services;
using LondonUndergroundSimulator.Services;
using LondonUndergroundSimulator.Training;
using LondonUndergroundSimulator.Training.Services;
using Microsoft.Extensions.DependencyInjection;
using MLInferenceEngine.Abstractions.Interfaces;
using MLInferenceEngine.Orchestration.Services;
using MLInferenceEngine.Services;
using System.Diagnostics;

namespace LondonUndergroundSimulator.HostServices
{
    public class SimulationHost
    {
        private  PythonInferenceLauncher _python;
        private readonly StartupChecks _checks;

        private readonly ITrainingService _trainingService;

        public SimulationHost(PythonInferenceLauncher python, 
                              StartupChecks checks,
                              ITrainingService trainingService)
        {
            _python = python;
            _checks = checks;
            _trainingService = trainingService;
        }

        public async Task FinaliseAsync()
        {
            var results = _checks.RunChecks().ToList();

            if (results.Any(x => !x.Passed))
            {
                //Debug.WriteLine("Startup checks failed. Starting training...");

                await _trainingService.TrainAsync();

                // Re-run checks after training
                results = _checks.RunChecks().ToList();
                foreach(var check in results)
                {
                    Debug.WriteLine($"Check: {check.Name}, Passed: {check.Passed}, Message: {check.Message}");
                }
                if (results.Any(x => !x.Passed))
                {
                    throw new InvalidOperationException(
                        "Startup checks still failing after training.");
                }
            }

            _python.Start();
        }

        public async Task Stop()
        {
            await _python?.Stop();
        }

        public async Task RetrainModelAsync(bool userInitiated)
        {
            await _python.Stop();

            await _trainingService.TrainAsync(userInitiated);

            _python.Start();
        }

    }
}
