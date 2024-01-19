using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class SignSymbol : UnarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new SignSymbol(symbols[0]);
            };
        }

        public SignSymbol(Symbol left) : base(left)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);

            if (double.IsNaN(l) || double.IsInfinity(l))
            {
                return l;
            }

            return Math.Sign(l);
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);
            
            return $"SIGN({l})";
        }

        public override Symbol Copy()
        {
            return new SignSymbol(Left.Copy());
        }

        public override Symbol Simplify()
        {
            if (GetVariables().Count == 0)
            {
                return new ConstSymbol(Eval(null));
            }

            Symbol l = Left.Simplify();

            if (l is SignSymbol)
            {
                return new SignSymbol(((SignSymbol)l).Left);
            }

            return new SignSymbol(l);
        }
    }
}
