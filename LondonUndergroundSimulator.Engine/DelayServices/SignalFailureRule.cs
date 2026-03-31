using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using System;

namespace LondonUndergroundSimulator.Engine.DelayServices
{
    /// <summary>
    /// Pure failure rule:
    /// - No side effects
    /// - Does NOT modify Train
    /// - Only decides whether a delay should trigger
    /// </summary>
    public class SignalFailureRule : IFailureRule
    {
        private readonly Random _rng = new();

        // Probability settings
        private readonly float _chancePerMinute;

        public string Name => "Signal Failure";

        public SignalFailureRule(float chancePerMinute = 0.09f)
        {
            _chancePerMinute = chancePerMinute;
        }

        /// <summary>
        /// Returns true if this failure should trigger this frame.
        /// Pure function: no side effects.
        /// </summary>
        public bool ShouldTrigger(float dt)
        {
            float chancePerSecond = _chancePerMinute / 60f;
            return _rng.NextDouble() < chancePerSecond * dt;
        }

        /// <summary>
        /// Returns the delay duration for this failure.
        /// Pure function: no side effects.
        /// </summary>
        public float GetDelayDuration(Train train)
        {
            return _rng.Next(20, 60); // 20–60 seconds
        }

    }
}