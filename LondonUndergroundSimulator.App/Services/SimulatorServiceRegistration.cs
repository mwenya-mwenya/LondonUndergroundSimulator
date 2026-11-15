using Microsoft.Extensions.DependencyInjection;
using DataPipeline;
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
using MLInferenceEngine.Abstractions.Interfaces;
using MLInferenceEngine.Orchestration.Services;
using MLInferenceEngine.Services;
using System.Diagnostics;

namespace LondonUndergroundSimulator.HostServices.Services
{
    public  class SimulatorServiceRegistration
    {

        public  void AddSimulatorServices(IServiceCollection services)
        {
            ConfigureStartupChecks(services);
            ConfigurePythonInferenceLauncher(services);
            ConfigureSimulationHost(services);

            ConfigureMlDataCollection(services);
            ConfigureMlTraining(services);

            ConfigureDataPipeline(services);
            ConfigureMLInference(services);
            ConfigureOrchestration(services);
            ConfigureEngineServices(services);
            ConfigureSimulationEngine(services);
            ConfigureLogging(services);
            
        }


        // -------------------------------------------------------
        // Configuration Sections
        // -------------------------------------------------------
        private void ConfigureStartupChecks(IServiceCollection services)
        {
            services.AddSingleton( sp => new StartupChecks(sp.GetRequiredService<IFilePathService>()));
        }   
        private void ConfigureSimulationHost(IServiceCollection services)
        {
            services.AddSingleton<SimulationHost>();     
        }
        private void ConfigurePythonInferenceLauncher(IServiceCollection services)
        {
            services.AddSingleton<PythonInferenceLauncher>();
        }
        private void ConfigureDataPipeline(IServiceCollection services)
        {
            services.AddSingleton<IDataPipeline, JsonDataPipeline>();
            //services.AddSingleton<IFilePathService, FilePathService>();
            services.AddSingleton<LogMetrics>();
            services.AddSingleton<SimulationLogStats>();

            services.AddSingleton<List<Line>>(sp =>
            {
                var pipeline = sp.GetRequiredService<IDataPipeline>();
                var paths = sp.GetRequiredService<IFilePathService>();
                var path = paths.GetDefaultDataFile("lines.json");
                return MapLoader.Load(path);
            });

            services.AddSingleton<List<Train>>(sp =>
            {
                var lines = sp.GetRequiredService<List<Line>>();
                return Trains.GetTrains(lines);
            });
        }
        private void ConfigureMLInference(IServiceCollection services)
        {
            services.AddSingleton<FrequencyInferenceClient>(sp => new FrequencyInferenceClient("http://localhost:50051"));
            services.AddSingleton<IInferenceStatusService, InferenceStatusService>();
        }

        private void ConfigureOrchestration(IServiceCollection services)
        {
            services.AddSingleton<IFrequencyAdvisor, FrequencyAdvisor>();
        }

        private void ConfigureEngineServices(IServiceCollection services)
        {
            services.AddSingleton<ITrainDelayService, TrainDelayService>();
            services.AddSingleton<IHeadwayService, HeadwayService>();
        }

        private void ConfigureMlDataCollection(IServiceCollection services)
        {
            services.AddSingleton<ILogReader, LogReader>();
            services.AddSingleton<IExporter, CsvExporter>();
            services.AddSingleton<IFilePathService, FilePathService>();

            services.AddSingleton<IAggregator, StationAggregator>();
            services.AddSingleton<IAggregator, TrainAggregator>();
            services.AddSingleton<IAggregator, LineAggregator>();
            services.AddSingleton<IAggregator, NetworkAggregator>();

        }

        private void ConfigureMlTraining(IServiceCollection services)
        {
            services.AddSingleton<Pipeline>();
            services.AddSingleton<ModelTrainer>();

            services.AddSingleton<TrainingHost>(sp =>
            {
                var pipeline = sp.GetRequiredService<Pipeline>();
                var trainer = sp.GetRequiredService<ModelTrainer>();
                var paths = sp.GetRequiredService<IFilePathService>();
                return new TrainingHost(pipeline, trainer, paths);
            });

            services.AddSingleton<ITrainingService>(sp =>
            {
                var host = sp.GetRequiredService<TrainingHost>();
                return new TrainingService(host);
            });

        }

        private void ConfigureSimulationEngine(IServiceCollection services)
        {
            services.AddSingleton<SimulationEngine>(sp =>
            {
                Debug.WriteLine("SimulationEngine ctor start");

                var trains = sp.GetRequiredService<List<Train>>();
                var lines = sp.GetRequiredService<List<Line>>();

                // 1. Create terminal loop BEFORE engine
                var terminalLoop = new TerminalLoopService();

                var engine = new SimulationEngine(
                    trains,
                    lines,
                    sp.GetRequiredService<ITrainDelayService>(),
                    null,
                    null,
                    null,
                    sp.GetRequiredService<IFrequencyAdvisor>(),
                    terminalLoop,
                    sp.GetRequiredService<LogMetrics>(),
                    sp.GetRequiredService<SimulationLogStats>(),
                    initialiseMovement: true
                );

                // 2. wire movement service
                terminalLoop.setMovementService(engine.MovementService);

                terminalLoop.setLinesService(engine.Lines);

                // 3. Register terminal loop
                services.AddSingleton<ITerminalLoopService>(terminalLoop);

                // 4. Build failure engine
                var headway = sp.GetRequiredService<IHeadwayService>();
                headway.setEngine(engine);

                var failureEngine = new FailureRuleEngine(headway);
                failureEngine.AddRule(new HeadwayRule());
                failureEngine.AddRule(new SignalFailureRule());
                engine.SetFailureEngine(failureEngine);

                // 5. Snapshot provider
                var snapshotProvider = new SimulationSnapshotProvider(engine);
                engine.SetSnapshotProvider(snapshotProvider);
                services.AddSingleton<ISimulationSnapshotProvider>(snapshotProvider);

                Debug.WriteLine("SimulationEngine created");
                return engine;
            });
        }
        private void ConfigureLogging(IServiceCollection services)
        {
            services.AddSingleton<SimulationLogger>();
        }
    }

}
