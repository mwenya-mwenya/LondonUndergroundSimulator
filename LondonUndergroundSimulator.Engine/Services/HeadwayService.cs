using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Services
{
    public class HeadwayService : IHeadwayService
    {
        private SimulationEngine _engine;

        //public HeadwayService(SimulationEngine engine)
        //{
        //    _engine = engine;
            
        //}
        private IReadOnlyList<Train> _trains => _engine.trainsIReadonly;
        //private List<Train> _trains => _engine._trains;
        //public Train GetTrainAhead(Train train)
        //{
        //    return _trains
        //        .Where(t => t.Line == train.Line && t.Direction == train.Direction)
        //        .Where(t =>
        //            t.CurrentStationIndex > train.CurrentStationIndex ||
        //            (t.CurrentStationIndex == train.CurrentStationIndex && t.Progress > train.Progress))
        //        .OrderBy(t => t.CurrentStationIndex)
        //        .ThenBy(t => t.Progress)
        //        .FirstOrDefault();
        //}

        public Train GetTrainAhead(Train train)
        {
            return _trains
                .Where(t => t != train && t.Track == train.Track)
                .Where(t =>
                    t.CurrentStationIndex > train.CurrentStationIndex ||
                    (t.CurrentStationIndex == train.CurrentStationIndex && t.Progress > train.Progress))
                .OrderBy(t => t.CurrentStationIndex)
                .ThenBy(t => t.Progress)
                .FirstOrDefault();
        }

        public float GetDistanceToTrainAhead(Train train)
        {
            var ahead = GetTrainAhead(train);
            if (ahead == null) return float.MaxValue;

            return Vector2.Distance(train.Position, ahead.Position);
        }

        public Train GetTrainBehind(Train train)
        {
            return _trains
                .Where(t => t.Line == train.Line && t.Direction == train.Direction)
                .Where(t =>
                    t.CurrentStationIndex < train.CurrentStationIndex ||
                    (t.CurrentStationIndex == train.CurrentStationIndex && t.Progress < train.Progress))
                .OrderByDescending(t => t.CurrentStationIndex)
                .ThenByDescending(t => t.Progress)
                .FirstOrDefault();
        }

        public bool CanSpawnTrain(Train train, float minSafeDistance)
        {
            var ahead = GetTrainAhead(train);
            if (ahead == null) return true;

            return GetDistanceToTrainAhead(train) > minSafeDistance;
        }

        public void setEngine(SimulationEngine engine)
        {
            _engine = engine;
        }
    }

}
