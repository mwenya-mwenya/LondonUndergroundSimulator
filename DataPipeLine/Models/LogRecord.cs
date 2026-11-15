using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPipeLine.Models
{

        public class LogRecord
        {
            // Raw logged values
            public int DayOfWeek { get; set; }     // 0 = Sunday, 1 = Monday, ...
            public int Hour { get; set; }          // 0–23
            public int Minute { get; set; }        // 0–59

            public string TrainId { get; set; }
            public string Station { get; set; }
            public string LineName { get; set; }
            public string StationType { get; set; }

            public int WaitingPassengers { get; set; }
            public int CurrentPassengers { get; set; }
            public int MaxCapacity { get; set; }
            public bool IsDwelling { get; set; }
            public bool IsDelayed { get; set; }

            // ---------------------------------------------------------
            // Derived ML features
            // ---------------------------------------------------------

            // Total minutes since midnight (used for window bucketing)
            public int TimeMinutes => Hour * 60 + Minute;

            // Cyclical time encoding (ML-friendly)
            public double HourSin => Math.Sin(2 * Math.PI * Hour / 24.0);
            public double HourCos => Math.Cos(2 * Math.PI * Hour / 24.0);

            public double MinuteSin => Math.Sin(2 * Math.PI * Minute / 60.0);
            public double MinuteCos => Math.Cos(2 * Math.PI * Minute / 60.0);

            public double DaySin => Math.Sin(2 * Math.PI * DayOfWeek / 7.0);
            public double DayCos => Math.Cos(2 * Math.PI * DayOfWeek / 7.0);

            // Peak/off‑peak flags
            public bool IsMorningPeak => Hour >= 7 && Hour < 9;
            public bool IsLunchPeak => Hour >= 12 && Hour < 14;
            public bool IsEveningPeak => Hour >= 17 && Hour < 19;
            public bool IsWeekend => DayOfWeek == 6 || DayOfWeek == 0;

            // Trend features (filled in by aggregator or pipeline)
            public double DeltaWaiting { get; set; }
            public double DeltaLoadFactor { get; set; }

            // Expected demand multiplier (from your simulator)
            public double ExpectedDemandMultiplier { get; set; }
        }
    


}
