using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLInferenceEngine.Abstractions.Interfaces
{
    public interface IModelStatusService
    {
        bool ModelExists { get; }
        bool IsDataOptimal { get; }
    }

}
