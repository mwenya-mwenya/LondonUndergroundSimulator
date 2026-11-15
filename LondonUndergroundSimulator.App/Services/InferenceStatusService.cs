using LondonUndergroundSimulator.HostServices.Interfaces;
using MLInferenceEngine.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.HostServices.Services
{
    public class InferenceStatusService : IInferenceStatusService
    {
        private readonly PythonInferenceLauncher _launcher;
        private readonly FrequencyInferenceClient _client;

        public InferenceStatusService(PythonInferenceLauncher launcher, FrequencyInferenceClient client)
        {
            _launcher = launcher;
            _client = client;
        }

        public async Task<bool> IsRunningAsync()
        {
            if (!await _launcher.IsRunningAsync())
            {
                Debug.WriteLine("_launcher.isRunning() is not running - _client.PinAsync not called");
                return false;
            }

            return await _client.PingAsync();
        }


    }

}
