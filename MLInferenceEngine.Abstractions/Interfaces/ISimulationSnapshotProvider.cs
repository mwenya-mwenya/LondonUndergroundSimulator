using MLInferenceEngine.Abstractions.Models;

namespace MLInferenceEngine.Abstractions.Interfaces
{

    public interface ISimulationSnapshotProvider
    {
        SimulationSnapshot CaptureSnapshot();
        

    }
}
