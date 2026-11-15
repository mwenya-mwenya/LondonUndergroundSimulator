using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPipeLine.Interfaces
{
    public interface IExporter
    {
        void Export<T>(string path, IEnumerable<T> data);
    }

}
