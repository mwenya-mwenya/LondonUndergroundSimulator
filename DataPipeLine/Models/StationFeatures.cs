using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPipeLine.Models
{

    public class StationFeatures
    {
        public override string ToString()
        {
            return $"{Station} → AvgWaiting={AvgWaiting}, MaxWaiting={MaxWaiting}, BusyPercentage={BusyPercentage}";
        }

        public string Station { get; set; }
        public double AvgWaiting { get; set; }
        public int MaxWaiting { get; set; }
        public double BusyPercentage { get; set; }
    }
}
