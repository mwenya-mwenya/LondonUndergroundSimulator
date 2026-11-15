using DataPipeLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPipeLine.Interfaces
{
    public interface ILogReader
    {
        IEnumerable<LogRecord> Read(string path);
    }

}
