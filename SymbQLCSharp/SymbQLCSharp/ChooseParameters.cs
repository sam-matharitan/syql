using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class ChooseParameters
    {
        public string[] VarNames { get; set; }

        public int ConstMinValue { get; set; } = 1;

        public int ConstMaxValue { get; set; } = 2;

        public ChooseParameters()
        {
        }

        public int IndexOfVar(string v)
        {
            for (int i = 0; i < VarNames.Length; i++)
            {
                if (VarNames[i] == v)
                {
                    return i;
                }
            }

            throw new Exception("Variable '" + v + "' does not exist in this context.");
        }
    }
}
