using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPipeLine.Models
{
    public class TrainFeatures
    {
        public override string ToString()
        {
            return $"{TrainId} → AvgLoadFactor={AvgLoadFactor}, DelayRate={DelayRate}, AvgDwell={AvgDwell}";
        }

        public string TrainId { get; set; }
        public double AvgLoadFactor { get; set; }

        public double DelayRate { get; set; }

        public int AvgDwell { get; set; }
    }
}
