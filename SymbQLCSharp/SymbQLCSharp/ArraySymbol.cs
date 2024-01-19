using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class ArraySymbol : Symbol
    {
        public Symbol[] Symbols { get; set; }

        public ArraySymbol(params Symbol[] symbols)
        {
            Symbols = symbols;
        }

        public override double Eval(double[] vars)
        {
            throw new NotImplementedException();
        }

        public double[] EvalArray(double[] vars)
        {
            return Symbols.Select(s => s.Eval(vars)).ToArray();
        }

        public override string ToExcelString(string[] vars)
        {
            return $"[{string.Join(", ", Symbols.Select(s => s.ToExcelString(vars)))}]";
        }

        public override Symbol Copy()
        {
            return new ArraySymbol(Symbols.Select(s => s.Copy()).ToArray());
        }

        public override Symbol Simplify()
        {
            return new ArraySymbol(Symbols.Select(s => s.Simplify()).ToArray());
        }

        public override int Weight()
        {
            return 1 + Symbols.Select(s => s.Weight()).Sum();
        }

        public override List<VarSymbol> GetVariables()
        {
            return Symbols.SelectMany(s => s.GetVariables()).ToList();
        }

        public override void Organize()
        {
            foreach (Symbol s in Symbols)
            {
                s.Organize();
            }
        }
    }
}
