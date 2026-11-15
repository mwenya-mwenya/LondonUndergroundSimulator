using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLInferenceEngine.Abstractions.Interfaces
{
    public interface ITrainingService
    {
        Task TrainAsync(bool userIniated = false);

    }

}
