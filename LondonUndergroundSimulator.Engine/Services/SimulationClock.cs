using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonUndergroundSimulator.Engine.Services
{
    public class SimulationClock
    {
        public int DayOfWeek { get; private set; } = 0; // 0 = Monday
        public int Hour { get; private set; } = 0;      // 0–23
        public int Minute { get; private set; } = 0;    // 0–59

        private float accumulatedSeconds = 0f;

        public void Advance(float deltaSeconds)
        {
            accumulatedSeconds += deltaSeconds;

            while (accumulatedSeconds >= 0.1f)
            {
                accumulatedSeconds -= 0.1f;
                Minute++;

                if (Minute >= 60)
                {
                    Minute = 0;
                    Hour++;

                    if (Hour >= 24)
                    {
                        Hour = 0;
                        DayOfWeek = (DayOfWeek + 1) % 7;
                    }
                }
            }
        }

        public override string ToString()
        {
            string day = DayOfWeek switch
            {
                0 => "Mon",
                1 => "Tue",
                2 => "Wed",
                3 => "Thu",
                4 => "Fri",
                5 => "Sat",
                6 => "Sun",
                _ => "?"
            };

            return $"{day} {Hour:D2}:{Minute:D2} {(Hour > 12 ? "PM" : "AM")}";
        }
    }

}
