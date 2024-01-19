using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class DataFileContext : DataContext
    {
        public string Filename { get; set; }

        public bool Header { get; set; }

        public DataFileContext(string filename, bool header = true)
        {
            Filename = filename;
            Header = header;
        }

        public override double[][] AllRows()
        {
            List<double[]> result = new List<double[]>();

            using (StreamReader reader = new StreamReader(Filename))
            {
                if (Header)
                {
                    string header = reader.ReadLine();
                }
                
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] parts = line.Split(new char[] { ',' });
                    result.Add(parts.Select(p => Double.Parse(p)).ToArray());
                }
            }

            return result.ToArray();
        }
    }
}
