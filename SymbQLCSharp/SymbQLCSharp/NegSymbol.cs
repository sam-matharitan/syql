using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class NegSymbol : UnarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new NegSymbol(symbols[0]);
            };
        }

        public NegSymbol(Symbol left) : base(left)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);

            return -l;
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);

            if (false
                || Left is AddSymbol
                || Left is MinusSymbol)
            {
                l = $"({l})";
            }

            return $"-{l}";
        }

        public override Symbol Copy()
        {
            return new NegSymbol(Left.Copy());
        }

        public override Symbol Simplify()
        {
            if (GetVariables().Count == 0)
            {
                return new ConstSymbol(Eval(null));
            }

            Symbol l = Left.Simplify();

            if (l is AddSubManySymbol)
            {
                return new AddSubManySymbol(((AddSubManySymbol)l).Subends, ((AddSubManySymbol)l).Addends);
            }

            return new NegSymbol(l);
        }
    }
}
