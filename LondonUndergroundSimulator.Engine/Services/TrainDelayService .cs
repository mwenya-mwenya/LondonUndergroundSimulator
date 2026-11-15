using LondonUndergroundSimulator.Engine.Interfaces;
using LondonUndergroundSimulator.Engine.Models;
using System;

namespace LondonUndergroundSimulator.Engine.Services
{
    /// <summary>
    /// Handles ONLY:
    /// - delay windows
    /// - delay start/end
    /// - delay cooldown
    /// - dwell logic
    /// </summary>
    public class TrainDelayService : ITrainDelayService
    {
        public bool IsTrainDelayed(Train train)
        {
            return train.IsDelayed;
        }

        /// <summary>
        /// Updates an active delay window.
        /// Returns true if the train is still delayed.
        /// </summary>
        public void UpdateDelay(Train train, float dt, TimeSpan clock)
        {
            train.DelayTimer -= dt;

            if (train.DelayTimer <= 0)
            {
                // Delay ends
                train.IsDelayed = false;
                train.DelayTimer = 0;
                train.DelayDuration = 0;
                train.DelayStartTime = TimeSpan.Zero;
                train.DelayReasons.Clear();

                train.LastDelayEndTime = clock;
            }
        }

        /// <summary>
        /// Applies a new delay from a triggered failure rule.
        /// </summary>
        public void ApplyDelay(Train train, IFailureRule rule, TimeSpan clock)
        {
            // Cooldown check
            if (clock < train.LastDelayEndTime + train.DelayCooldown)
                return;

            train.IsDelayed = true;

            float duration = rule.GetDelayDuration(train);
            train.DelayDuration = duration;
            train.DelayTimer = duration;

            train.DelayStartTime = clock;
            train.DelayReasons.Add(rule.Name);
     
        }

        public bool UpdateDwell(Train train, float dt, Station station)
        {
            if (!train.IsDwelling)
                return false;

            // FIRST TICK OF DWELL
            if (!train.HasProcessedDwell)
            {
                UnloadPassengers(train);
                LoadPassengers(train, station);

                train.HasProcessedDwell = true;
            }

            train.DwellTimer -= dt;

            if (train.DwellTimer <= 0)
            {
                train.IsDwelling = false;
                train.DwellTimer = 0;
                train.HasProcessedDwell = false; // reset for next station
                return false;
            }

            return true;
        }


        public void UnloadPassengers(Train train)
        {
            var random = new Random();
            double unloadPercentage = random.NextDouble() * 0.3; // up to 30%

            int passengersToUnload = (int)(train.CurrentPassengers * unloadPercentage);
            train.CurrentPassengers -= passengersToUnload;

            // Optionally: track total passengers unloaded at this station
        }

        public void LoadPassengers(Train train, Station station)
        {         

            int availableCapacity = train.MaxCapacity - train.CurrentPassengers;
            if (availableCapacity <= 0) return;

            int passengersToLoad = Math.Min(availableCapacity, station.WaitingPassengers);
       
            train.CurrentPassengers += passengersToLoad;
            station.WaitingPassengers -= passengersToLoad;
         

        }


    }
}