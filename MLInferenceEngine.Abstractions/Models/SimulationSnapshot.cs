using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLInferenceEngine.Abstractions.Models
{
    public class SimulationSnapshot
    {
        // ---------------------------------------------------------
        // Operational metrics
        // ---------------------------------------------------------
        public double AvgWaiting { get; set; }
        public double MaxWaiting { get; set; }
        public double AvgLoadFactor { get; set; }
        public int CurrentTrainCount { get; set; }

        // ---------------------------------------------------------
        // Raw time-of-day
        // ---------------------------------------------------------
        public int DayOfWeek { get; set; }   // 0 = Sunday
        public int Hour { get; set; }        // 0–23
        public int Minute { get; set; }      // 0–59
        public int TimeMinutes { get; set; } // Hour*60 + Minute

        // ---------------------------------------------------------
        // Cyclical time encoding (ML-friendly)
        // ---------------------------------------------------------
        public double HourSin { get; set; }
        public double HourCos { get; set; }

        public double MinuteSin { get; set; }
        public double MinuteCos { get; set; }

        public double DaySin { get; set; }
        public double DayCos { get; set; }

        // ---------------------------------------------------------
        // Peak/off‑peak flags
        // ---------------------------------------------------------
        public bool IsMorningPeak { get; set; }
        public bool IsLunchPeak { get; set; }
        public bool IsEveningPeak { get; set; }
        public bool IsWeekend { get; set; }

        // ---------------------------------------------------------
        // Trend features (optional, activated later)
        // ---------------------------------------------------------
        public double DeltaWaiting { get; set; }
        public double DeltaLoadFactor { get; set; }

        // ---------------------------------------------------------
        // Simulator-derived demand multiplier
        // ---------------------------------------------------------
        public double ExpectedDemandMultiplier { get; set; }
    }
}

