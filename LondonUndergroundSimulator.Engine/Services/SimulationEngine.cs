using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Logging;
using LondonUndergroundSimulator.Engine.Models;
using MLInferenceEngine.Abstractions.Interfaces;
using System.ComponentModel;
using System.Diagnostics;
using static LondonUndergroundSimulator.Engine.Models.Station;
using static System.Collections.Specialized.BitVector32;
using LondonUndergroundSimulator.Infrastructure.Interfaces;
using LondonUndergroundSimulator.Infrastructure.Services;
using MLInferenceEngine.Abstractions.Models;

namespace LondonUndergroundSimulator.Engine.Services
{
    /// <summary>
    /// High‑level orchestrator responsible ONLY for:
    /// - advancing simulation time
    /// - delegating movement, delays, and failure evaluation
    /// </summary>
    /// 

    public class SimulationEngine
    {
        public readonly ITrainMovementService _movementService;
        private readonly ITrainDelayService _delayService;
        private IFailureRuleEngine _failureEngine;

        private IHeadwayService _headwayService;

        private ITerminalLoopService _terminalLoopService;

        private ISimulationSnapshotProvider _snapshotProvider { get; set; }
        
        private readonly IFrequencyAdvisor _frequencyAdvisor;

        private readonly SimulationLogStats _logStats;

        public MLConsoleData MLConsoleData = new MLConsoleData();

        public IReadOnlyList<Train> trainsIReadonly => _trains;
        public readonly List<Train> _trains;

        public IReadOnlyList<Line> Lines { get; }

        public ITrainMovementService MovementService => _movementService;
        public float TimeScale { get; set; } = 4f;
        public float SimulationTime { get; private set; } = 0f;
        public TimeSpan Clock => TimeSpan.FromSeconds(SimulationTime);
        public SimulationClock Clock24 { get; private set; } = new SimulationClock();

        private TimeSpan _lastPassengerGenTime = TimeSpan.Zero;
        private TimeSpan _lastLogTime;

        private int? _pendingMLRecommendation;
        private TimeSpan _lastLogTimeMLFrequency;

        //private readonly LogMetrics _logMetrics = new LogMetrics(new FilePathService());
        private readonly LogMetrics _logMetrics;
        //public event Action<int>? FrequencyChanged;
        public event Action FleetChanged;

        public event Action? RetrainingStatusChanged;

        private int _lastAppliedFrequency = -1;
        private Dictionary<string, int> _pendingLineRecommendations;

        public float PassengerDemandMultiplier { get; set; } = 1.0f;

        public SimulationEngine(
            List<Train> trains,
            List<Line> lines,
            ITrainDelayService delayService,
            IFailureRuleEngine failureEngine,
            IHeadwayService headwayService,
            ISimulationSnapshotProvider snapshotProvider,
            IFrequencyAdvisor frequencyAdvisor,
            ITerminalLoopService terminalLoopService,
            LogMetrics logMetrics,
            SimulationLogStats logStats,
            bool initialiseMovement = true
            //Train trainS
            )
        {
            _trains = trains;
            Lines = lines;
            _delayService = delayService;
            _failureEngine = failureEngine;
            _headwayService = headwayService;
            _movementService = new TrainMovementService(this, terminalLoopService);
            _terminalLoopService = terminalLoopService;
            _snapshotProvider = snapshotProvider;
            _frequencyAdvisor = frequencyAdvisor;
            _logMetrics = logMetrics;
            _logStats = logStats;
            
            if (initialiseMovement)
            {
                foreach (var train in _trains)
                    _movementService.InitialiseTrain(train);
            }
           
        }



        /// <summary>
        /// Advances the simulation by deltaTime (scaled).
        /// </summary>
        public void Update(float deltaTime)
        {
            SimulationTime += deltaTime * TimeScale;
            float scaledDelta = deltaTime * TimeScale;
            Clock24.Advance(deltaTime * TimeScale);
            ProcessFleetAdjustments();
            if (!CheckLinesForServiceStatus())
            {
                ResetWaitingPassengers();
                return;
            }
            if (Clock - _lastPassengerGenTime >= TimeSpan.FromSeconds(5))
            {
                GeneratePassengers(Clock24);
                _lastPassengerGenTime = Clock;
            }

            if (Clock - _lastLogTime >= TimeSpan.FromSeconds(3))
            {
                int logRowCount = _logStats.GetRowCount();
                long logFileSize = _logStats.GetFileSize();
                int minimumBytes = 100 * 1024; // 100 KB
                string lastTimeModelUpdated = _logStats.GetDateModelLastUpdate();

                _logMetrics.LogSnapshot(_trains, Clock24);
                MLConsoleData.SimulationLogRows = logRowCount;
                MLConsoleData.SimulationLogSizeBytes = logFileSize;
                MLConsoleData.LastTimeModelUpdated = lastTimeModelUpdated;
                MLConsoleData.TrainingReadinessPercent = (int)Math.Min(100, (int)(logFileSize * 100.0 / minimumBytes));

                if (logRowCount > 500 && logFileSize > minimumBytes)
                {
                    RetrainingStatusChanged?.Invoke();
                }
                else
                {
                    RetrainingStatusChanged?.Invoke();
                }

                _lastLogTime = Clock;
            }

            if (Clock - _lastLogTimeMLFrequency >= TimeSpan.FromSeconds(30))
            {
                _lastLogTimeMLFrequency = Clock;
                var snapshots = _snapshotProvider.CaptureSnapshots();
                _ = UpdateFrequencyAsync(snapshots, Clock24);
            }

            foreach (var train in _trains)
            {
                // 1. Handle active delay window
                if (_delayService.IsTrainDelayed(train))
                {
                    _delayService.UpdateDelay(train, scaledDelta, Clock);
                    if (train.IsDelayed)
                        continue; // still delayed → skip movement
                }

                // 2. Evaluate failure rules (if not delayed)
                var triggeredRule = _failureEngine.Evaluate(train, scaledDelta);
                if (triggeredRule != null)
                {
                    _delayService.ApplyDelay(train, triggeredRule, Clock);
                    continue; // delay begins → skip movement
                }

                // 3. Handle dwell logic
                if (_delayService.UpdateDwell(train, scaledDelta, train.Line.Stations[train.CurrentStationIndex]))
                    continue; // still dwelling → skip movement

                // 4. Delegate movement + geometry
                _movementService.UpdateMovement(train, scaledDelta);
                _movementService.UpdateGeometry(train, scaledDelta);
            }
        }
        private readonly Random _random = new Random();

        private void GeneratePassengers(SimulationClock clock)
        {
            float timeMultiplier = GetTimeOfDayMultiplier(clock);

            float multiplier =
                timeMultiplier * PassengerDemandMultiplier;

            if (!CheckLinesForServiceStatus())
            {
                return;
            }

            foreach (var line in Lines)
            {

                foreach (var station in line.Stations)
                {
                    int newPassengers = station.Type switch
                    {
                        StationType.Quiet =>
                            (int)(_random.Next(0, 2) * multiplier),

                        StationType.Normal =>
                            (int)(_random.Next(0, 5) * multiplier),

                        StationType.Busy =>
                            (int)(_random.Next(5, 15) * multiplier),

                        _ => 0
                    };

                    station.WaitingPassengers += newPassengers;
                }
            }
        }

        public bool CheckLinesForServiceStatus()
        {
            foreach (var line in Lines)
            {
                if (line.IsServiceActive)
                    return true;
            }
            return false;
        }

        public void ResetWaitingPassengers()
        {
            foreach (var line in Lines)
            {
                foreach (var station in line.Stations)
                {
                    station.WaitingPassengers = 0;
                }
            }
        }

        public float GetTimeOfDayMultiplier(SimulationClock clock)
        {
            int h = clock.Hour;

            if (clock.DayOfWeek is 1 or 2 or 3 or 4 or 5) // Weekday
            {
                if (h is >= 7 and < 9) return 3.5f;      // Morning peak
                if (h is >= 12 and < 14) return 2f;      // Lunch tourist peak
                if (h is >= 17 and < 19) return 3f;      // Evening peak
                return 1f;                               // Off-peak
            }

            if (clock.DayOfWeek == 6) // Saturday
            {
                if (h is >= 11 and < 18) return 2f;
                return 0.7f;
            }

            if (clock.DayOfWeek == 0) // Sunday
            {
                if (h is >= 12 and < 16) return 1.2f;
                return 0.5f;
            }

            return 1f;
        }


        private async Task UpdateFrequencyAsync(IReadOnlyList<SimulationSnapshot> snapshots, SimulationClock clock)
        {
            try
            {
                var recommendations = new Dictionary<string, int>();

                foreach (var snapshot in snapshots)
                {
                    var lineName = snapshot.LineName;
                    var recommended = await _frequencyAdvisor.GetRecommendedFrequencyAsync(lineName, snapshot);
                    if (recommended > 10)
                    {
                        recommended = Lines.FirstOrDefault(l => l.Name == lineName)?.MaxSafeTrainCount ?? 10;
                    }
                    recommendations[lineName] = recommended;

                    MLConsoleData.LatestSnapshot = snapshot;
                    MLConsoleData.LatestRecommendation = recommended;
                    MLConsoleData.Log.Add(new MLConsoleLogEntry
                    {
                        Time = clock.ToString(),
                        Message = $"[{snapshot.LineName}] Rec'd trains: {recommended}"
                    });
                }

                // Hand off the per-line dictionary to the main Update() tick
                _pendingLineRecommendations = recommendations;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ML] Frequency inference failed: {ex.Message}");
            }

        }

        public void ApplyFrequencyChange(int recommended)
        {
            if (_pendingMLRecommendation.HasValue)
            {
                int rec = _pendingMLRecommendation.Value;
                _pendingMLRecommendation = null;

                if (rec != _lastAppliedFrequency)
                {
                    ApplyFrequencyChange(rec);
                    _lastAppliedFrequency = rec;
                }
            }

            foreach (var line in Lines)
            {
                if (_pendingLineRecommendations != null && _pendingLineRecommendations.TryGetValue(line.Name, out int recommendedForLine))
                {
                    line.TargetNumberOfTrains = Math.Clamp(recommendedForLine, 1, line.MaxSafeTrainCount);
                }
            }

        }

        public void ProcessFleetAdjustments()
        {
            foreach (var line in Lines)
            {

                if (_pendingLineRecommendations != null && _pendingLineRecommendations.TryGetValue(line.Name, out int recommendedForLine))
                {
                    line.TargetNumberOfTrains = Math.Clamp(recommendedForLine, 1, line.MaxSafeTrainCount);
                }
                var lineTrains = _trains
                    .Where(t => t.Line == line)
                    .ToList();
                int activeCount = lineTrains.Count;

                if (activeCount < line.TargetNumberOfTrains)
                {

                    var eastboundCount = lineTrains.Count(t => t.Track == line.Eastbound);
                    var westboundCount = lineTrains.Count(t => t.Track == line.Westbound);

                    var targetTrack =
                        eastboundCount <= westboundCount
                            ? line.Eastbound
                            : line.Westbound;
                    bool trainAdded = false;

                    if (Trains.CanSpawnTrain(targetTrack, minSafeDistance: 50f, _trains))
                    {
                        var newTrain = Trains.CreateSingleTrain(line, targetTrack);
                        _movementService.InitialiseTrain(newTrain);
                        _trains.Add(newTrain);
                        trainAdded = true;

                        if (trainAdded)
                        {
                            FleetChanged?.Invoke();
                        }

                    }
                }
                else if (activeCount > line.TargetNumberOfTrains)
                {

                    var trainToRemove = lineTrains
                        .OrderByDescending(t => t.CurrentStationIndex)
                        .ThenByDescending(t => t.Progress)
                        .FirstOrDefault();

                    if (trainToRemove != null)
                    {
                        _trains.Remove(trainToRemove);
                        FleetChanged?.Invoke();
                    }
                } 

                line.NumberOfTrains = _trains.Count(t => t.Line == line);
            }
        }

        public void SetFailureEngine(IFailureRuleEngine engine)
        {
            _failureEngine = engine;
        }

        public void SetTerminalLoopService(TerminalLoopService terminalLoopService)
        {
            _terminalLoopService = terminalLoopService;
        }

        public void SetSnapshotProvider(ISimulationSnapshotProvider provider)
        {
            _snapshotProvider = provider;
        }

    }

}