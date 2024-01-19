using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class SqrtSymbol : UnarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new SqrtSymbol(symbols[0]);
            };
        }

        public SqrtSymbol(Symbol left) : base(left)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);

            return Math.Sqrt(l);
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);

            return $"SQRT({l})";
        }

        public override Symbol Copy()
        {
            return new SqrtSymbol(Left.Copy());
        }

        public override Symbol Simplify()
        {
            if (GetVariables().Count == 0)
            {
                return new ConstSymbol(Eval(null));
            }

            Symbol l = Left.Simplify();

            if (l is SquareSymbol)
            {
                return ((SquareSymbol)l).Left;
            }

            if (l is SqrtSymbol)
            {
                return new PowSymbol(((SqrtSymbol)l).Left, (Symbol).25);
            }

            if (l is PowSymbol)
            {
                PowSymbol ps = (PowSymbol)l;
                MultSymbol newRight = new MultSymbol(ps.Right, (Symbol).5);

                return (new PowSymbol(ps.Left, newRight)).Simplify();
            }

            if (l is ConstSymbol)
            {
                return new ConstSymbol(Math.Sqrt(((ConstSymbol)l).Value));
            }

            return new SqrtSymbol(l);
        }
    }
}
