using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class CosSymbol : UnarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new CosSymbol(symbols[0]);
            };
        }

        public CosSymbol(Symbol left) : base(left)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);

            return Math.Cos(l);
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);

            return $"COS({l})";
        }

        public override Symbol Copy()
        {
            return new CosSymbol(Left.Copy());
        }

        public override Symbol Simplify()
        {
            return Copy();
        }
    }
}
