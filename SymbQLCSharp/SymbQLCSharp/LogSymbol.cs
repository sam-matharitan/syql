using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class LogSymbol : UnarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new LogSymbol(symbols[0]);
            };
        }

        public LogSymbol(Symbol left) : base(left)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);

            return Math.Log(l);
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);

            return $"LN({l})";
        }

        public override Symbol Copy()
        {
            return new LogSymbol(Left.Copy());
        }

        public override Symbol Simplify()
        {
            if (GetVariables().Count == 0)
            {
                return new ConstSymbol(Eval(null));
            }

            Symbol l = Left.Simplify();

            if (l is ExpSymbol)
            {
                return ((ExpSymbol)l).Left;
            }

            if (l is PowSymbol)
            {
                PowSymbol ps = (PowSymbol)l;

                return (new MultSymbol(ps.Right, new LogSymbol(ps.Left))).Simplify();
            }

            return new LogSymbol(l);
        }
    }
}
