using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.DelayServices
{
    public class HeadwayRule : IFailureRule
    {
        public string Name => "Headway Block";

        private readonly float _minimumSpacing; // meters

        public HeadwayRule(float minimumSpacing = 40f)
        {
            _minimumSpacing = minimumSpacing;
        }

        public bool ShouldTrigger(float dt, Train train, Train trainAhead)
        {
            if (trainAhead == null)
                return false;

            float dist = Vector2.Distance(train.Position, trainAhead.Position);

            return dist < _minimumSpacing;
        }

        public float GetDelayDuration(Train train)
        {
            return 5f; // small pause until spacing increases
        }


    }

}
