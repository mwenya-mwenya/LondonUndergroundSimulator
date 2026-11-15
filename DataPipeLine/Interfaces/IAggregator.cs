using DataPipeLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPipeLine.Interfaces
{
    //public interface IAggregator<out T>
    //{
    //    IEnumerable<T> Aggregate(IEnumerable<LogRecord> records);
    //}

    public interface IAggregator
    {
        IEnumerable<object> AggregateUntyped(IEnumerable<LogRecord> records);
    }

    public interface IAggregator<out T> : IAggregator
    {
        IEnumerable<T> Aggregate(IEnumerable<LogRecord> records);

        IEnumerable<object> IAggregator.AggregateUntyped(IEnumerable<LogRecord> records)
            => Aggregate(records).Cast<object>();
    }



}
