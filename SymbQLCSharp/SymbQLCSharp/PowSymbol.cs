using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class PowSymbol : BinarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new PowSymbol(symbols[0], symbols[1]);
            };
        }

        public PowSymbol(Symbol left, Symbol right) : base(left, right)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);
            double r = Right.Eval(vars);

            return Math.Pow(l, r);
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);
            string r = Right.ToExcelString(vars);

            if (!(Left is ConstSymbol) && !(Left is VarSymbol))
            {
                l = $"({l})";
            }

            if (!(Right is ConstSymbol) && !(Right is VarSymbol))
            {
                r = $"({r})";
            }

            return $"{l}^{r}";
        }

        public override Symbol Copy()
        {
            return new PowSymbol(Left.Copy(), Right.Copy());
        }

        public override Symbol Simplify()
        {
            if (GetVariables().Count == 0)
            {
                return new ConstSymbol(Eval(null));
            }

            Symbol l = Left.Simplify();
            Symbol r = Right.Simplify();

            return new PowSymbol(l, r);
        }
    }
}
