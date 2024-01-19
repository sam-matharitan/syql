using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class DataRawContext : DataContext
    {
        public double[][] Data { get; set; }

        public DataRawContext(double[][] raw)
        {
            Data = raw;
        }

        public override double[][] AllRows()
        {
            return Data;
        }
    }
}
