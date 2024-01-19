using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class DivSymbol : BinarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new DivSymbol(symbols[0], symbols[1]);
            };
        }

        public DivSymbol(Symbol left, Symbol right) : base(left, right)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);
            double r = Right.Eval(vars);

            return l / r;
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);
            string r = Right.ToExcelString(vars);

            if (false
                || Left is AddSymbol
                || Left is MinusSymbol)
            {
                l = $"({l})";
            }

            if (false
                || Right is AddSymbol
                || Right is MinusSymbol
                || Right is MultSymbol
                || Right is DivSymbol)
            {
                r = $"({r})";
            }

            return $"{l}/{r}";
        }

        public override Symbol Copy()
        {
            return new DivSymbol(Left.Copy(), Right.Copy());
        }

        public override Symbol Simplify()
        {
            if (GetVariables().Count == 0)
            {
                return new ConstSymbol(Eval(null));
            }

            Symbol l = Left.Simplify();
            Symbol r = Right.Simplify();

            List<Symbol> syms = new List<Symbol>();
            List<Symbol> symsSub = new List<Symbol>();
            if (l is MultDivManySymbol)
            {
                syms.AddRange(((MultDivManySymbol)l).Factors);
                symsSub.AddRange(((MultDivManySymbol)l).UnderFactors);
            }
            else
            {
                syms.Add(l);
            }

            if (r is MultDivManySymbol)
            {
                syms.AddRange(((MultDivManySymbol)r).UnderFactors);
                symsSub.AddRange(((MultDivManySymbol)r).Factors);
            }
            else
            {
                symsSub.Add(r);
            }

            return (new MultDivManySymbol(syms.ToArray(), symsSub.ToArray())).Simplify();
        }
    }
}
