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
        private  IFailureRuleEngine _failureEngine;

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
        public event Action<int>? FrequencyChanged;

        public event Action? RetrainingStatusChanged;

        private int _lastAppliedFrequency = -1;

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

                if (logRowCount > 5000 && logFileSize > minimumBytes)
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
                _ = UpdateFrequencyAsync(Clock24);
            }




            foreach (var train in _trains)
            {
                

                if (train.Id == "Piccadilly_1")
                {
                    Debug.WriteLine($"[DEBUG] Train {train.Id} - Progress: {train.Progress}, Position: {train.Position}, Speed: {train.Speed}, CurrentStationIndex: {train.CurrentStationIndex}, IsDwelling: {train.IsDwelling}, DwellTimer: {train.DwellTimer}");
                }
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


                //if (availableCapacity <= 0) return;

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

            foreach (var line in Lines)
            {
                if (!line.IsServiceActive)
                    continue;

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

        
        private async Task UpdateFrequencyAsync(SimulationClock clock)
        {
            var snapshot = _snapshotProvider.CaptureSnapshot();
            var recommended = await _frequencyAdvisor.GetRecommendedFrequencyAsync(snapshot);
          
            // Update console snapshot
            MLConsoleData.LatestSnapshot = snapshot;
            MLConsoleData.LatestRecommendation = recommended;
            _pendingMLRecommendation = recommended;

            if (recommended != _lastAppliedFrequency)
            {
                ApplyFrequencyChange(recommended);
                FrequencyChanged?.Invoke(recommended);

                MLConsoleData.LastAppliedFrequency = recommended;

                MLConsoleData.Log.Add(new MLConsoleLogEntry
                {
                    Time = clock.ToString(),
                    Message = $"Applied new frequency: {recommended}"
                });

                _lastAppliedFrequency = recommended;
            }
            else
            {
                MLConsoleData.Log.Add(new MLConsoleLogEntry
                {
                    Time = clock.ToString(),
                    Message = $"Frequency unchanged ({recommended})"
                });

                Debug.WriteLine($"[ML] Frequency unchanged ({recommended}), skipping update");
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

            foreach (var train in _trains)
                ProcessFleetAdjustments(train);


            foreach (var line in Lines)
            {
                line.TargetNumberOfTrains = Math.Clamp(recommended, 1, line.MaxSafeTrainCount);

                int current = line.NumberOfTrains;

                if (recommended > current)
                {
                    int toAdd = recommended - current;

                    // ✔ Correct call
                    var newTrains = Trains.CreateAdditionalTrains(line, toAdd);
                    //var newTrains = Trains.CreateSingleTrain(line, line.Eastbound);
                    //var newTrains = Trains.CreateAdditionalTrains(line, toAdd);

                    foreach (var t in newTrains)
                    {
                        _movementService.InitialiseTrain(t);
                        _trains.Add(t);   // ✔ modify the private list, not the IReadOnlyList
                    }

                    line.NumberOfTrains = recommended;
                }
                else if (recommended < current)
                {
                    int toRemove = current - recommended;
                    RemoveLastTrains(_trains, line, toRemove);
                    line.NumberOfTrains = recommended;
                }
            }
        }


        public void ProcessFleetAdjustments(Train train)
        {
            foreach (var line in Lines)
            {
                int activeCount = _trains.Count(t => t.Line == line);

                // ADD ONE TRAIN AT A TIME WHEN CLEAR
                if (activeCount < line.TargetNumberOfTrains)
                {
                    // Alternate between Eastbound and Westbound, or pick the track with fewer trains
                    var targetTrack = _trains.Count(t => t.Track == line.Eastbound) <= _trains.Count(t => t.Track == line.Westbound)
                        ? line.Eastbound
                        : line.Westbound;

                    // Only spawn if Station 0 is clear of other trains!
                    if (_headwayService.CanSpawnTrain(train, minSafeDistance: 50f))
                    {
                        var newTrain = Trains.CreateSingleTrain(line, targetTrack);
                        _movementService.InitialiseTrain(newTrain);
                        _trains.Add(newTrain);
                        line.NumberOfTrains = activeCount + 1;
                    }
                }
                // REMOVE EXCESS TRAINS SAFELY AT TERMINUS (OR BY LINE)
                else if (activeCount > line.TargetNumberOfTrains)
                {
                    // Remove a train belonging to THIS specific line (ideally one at the end of a loop)
                    var trainToRemove = _trains
                        .Where(t => t.Line == line)
                        .OrderByDescending(t => t.CurrentStationIndex)
                        .FirstOrDefault();

                    if (trainToRemove != null)
                    {
                        _trains.Remove(trainToRemove);
                        line.NumberOfTrains = activeCount - 1;
                    }
                }
            }
        }


        private void RemoveLastTrains(List<Train> trains, Line line, int toRemove)
        {
            //for (int i = 0; i < toRemove; i++)
            //{
            //    if (_trains.Count == 0)
            //        break;

            //    _trains.RemoveAt(_trains.Count - 1);
            //}

            var removed = new List<Train>();

            // Prefer removing trains on this line that are closest to finishing their loop
            var candidates = trains
                .Where(t => t.Line == line)
                .OrderByDescending(t => t.CurrentStationIndex)
                .ThenByDescending(t => t.Progress)
                .Take(toRemove)
                .ToList();

            foreach (var t in candidates)
            {
                trains.Remove(t);
                removed.Add(t);
            }

            _trains.RemoveAll(t => removed.Contains(t));

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