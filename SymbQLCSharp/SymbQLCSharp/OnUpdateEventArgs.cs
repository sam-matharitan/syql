using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class OnUpdateEventArgs : EventArgs
    {
        public Model[] Models { get; set; }

        public int Batch { get; set; }

        public OnUpdateEventArgs(Model[] models, int batch)
        {
            Models = models;
            Batch = batch;
        }
    }
}
