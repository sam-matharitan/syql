using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class VarSymbol : Symbol
    {
        public static SymbolConnector Choose(ChooseParameters parameters)
        {
            int v = RNG.Random().Next(parameters.VarNames.Length);

            return (Symbol[] symbols) =>
            {
                return new VarSymbol(v);
            };
        }

        public static Chooser BuildChooser(int[] indexes)
        {
            return (ChooseParameters p) =>
            {
                int v = indexes[RNG.Random().Next(indexes.Length)];

                return (Symbol[] symbols) =>
                {
                    return new VarSymbol(v);
                };
            };
        }

        public int Index { get; set; }

        public VarSymbol(int index)
        {
            Index = index;
        }

        public override double Eval(double[] vars)
        {
            return vars[Index];
        }

        public override string ToExcelString(string[] vars)
        {
            return vars[Index];
        }

        public override Symbol Copy()
        {
            return new VarSymbol(Index);
        }

        public override Symbol Simplify()
        {
            return Copy();
        }

        public override int Weight()
        {
            return 1;
        }

        public override List<VarSymbol> GetVariables()
        {
            List<VarSymbol> vars = new List<VarSymbol>();
            vars.Add(this);

            return new List<VarSymbol>(vars);
        }

        public override void Organize()
        {
            return;
        }
    }
}
