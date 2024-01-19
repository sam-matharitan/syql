using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class ArgmaxSoftmaxSymbol : Symbol
    {
        public Symbol[] Symbols { get; set; }

        public ArgmaxSoftmaxSymbol(params Symbol[] symbols)
        {
            Symbols = symbols;
        }

        public override double Eval(double[] vars)
        {
            var exps = Symbols.Select(e => e.Eval(vars));
            double sum = exps.Select(e => Math.Exp(e)).Sum();
            List<double> probabilities = exps.Select(e => Math.Exp(e) / sum).ToList();

            return probabilities.IndexOf(probabilities.Max());
        }
        
        public override string ToExcelString(string[] vars)
        {
            return $"ARGMAX(SOFTMAX([{string.Join(", ", Symbols.Select(s => s.ToExcelString(vars)))}]))";
        }

        public override Symbol Copy()
        {
            return new ArgmaxSoftmaxSymbol(Symbols.Select(s => s.Copy()).ToArray());
        }

        public override Symbol Simplify()
        {
            return new ArgmaxSoftmaxSymbol(Symbols.Select(s => s.Simplify()).ToArray());
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
