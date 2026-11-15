using MLInferenceEngine.Abstractions.Models;
using System.Threading.Tasks;
using MLInferenceEngine.Abstractions;

namespace MLInferenceEngine.Abstractions.Interfaces
{
    public interface IFrequencyAdvisor
    {
        Task<int> GetRecommendedFrequencyAsync(SimulationSnapshot snapshot);
    }

}
