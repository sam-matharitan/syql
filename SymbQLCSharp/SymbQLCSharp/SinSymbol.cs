using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class SinSymbol : UnarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new SinSymbol(symbols[0]);
            };
        }

        public SinSymbol(Symbol left) : base(left)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);

            return Math.Sin(l);
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);

            return $"SIN({l})";
        }

        public override Symbol Copy()
        {
            return new SinSymbol(Left.Copy());
        }

        public override Symbol Simplify()
        {
            return Copy();
        }
    }
}
