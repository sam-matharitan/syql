using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class SquareSymbol : UnarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new SquareSymbol(symbols[0]);
            };
        }

        public SquareSymbol(Symbol left) : base(left)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);

            return l * l;
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);

            if (!(Left is ConstSymbol) && !(Left is VarSymbol))
            {
                l = $"({l})";
            }

            return $"{l}^2";
        }

        public override Symbol Copy()
        {
            return new SquareSymbol(Left.Copy());
        }

        public override Symbol Simplify()
        {
            if (GetVariables().Count == 0)
            {
                return new ConstSymbol(Eval(null));
            }

            Symbol l = Left.Simplify();

            if (l is SqrtSymbol)
            {
                return ((SqrtSymbol)l).Left;
            }

            if (l is SquareSymbol)
            {
                return new PowSymbol(((SquareSymbol)l).Left, (Symbol)4);
            }

            if (l is PowSymbol)
            {
                PowSymbol ps = (PowSymbol)l;
                MultSymbol newRight = new MultSymbol(ps.Right, (Symbol)2);

                return (new PowSymbol(ps.Left, newRight)).Simplify();
            }

            if (l is ConstSymbol)
            {
                return new ConstSymbol(Math.Pow(((ConstSymbol)l).Value, 2));
            }

            return new SquareSymbol(l);
        }
    }
}
