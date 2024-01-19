using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class ExpSymbol : UnarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new ExpSymbol(symbols[0]);
            };
        }

        public ExpSymbol(Symbol left) : base(left)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);

            return Math.Exp(l);
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);

            return $"EXP({l})";
        }

        public override Symbol Copy()
        {
            return new ExpSymbol(Left.Copy());
        }

        public override Symbol Simplify()
        {
            if (GetVariables().Count == 0)
            {
                return new ConstSymbol(Eval(null));
            }

            Symbol l = Left.Simplify();

            // EXP(LN(X)) = X
            if (l is LogSymbol)
            {
                return ((LogSymbol)l).Left;
            }

            // EXP(C1) = C2
            if (l is ConstSymbol)
            {
                return new ConstSymbol(Math.Exp(((ConstSymbol)l).Value));
            }

            // EXP(X*LN(Y)) = Y ^ X
            if (l is MultDivManySymbol)
            {
                MultDivManySymbol mms = (MultDivManySymbol)l;
                bool logFound = false;
                LogSymbol log = null;
                List<Symbol> others = new List<Symbol>();
                foreach(Symbol s in mms.Factors)
                {
                    if (logFound)
                    {
                        others.Add(s);
                    }
                    else if (s is LogSymbol)
                    {
                        logFound = true;
                        log = (LogSymbol)s;
                    }
                    else
                    {
                        others.Add(s);
                    }
                }

                if (logFound)
                {
                    if (others.Count > 1)
                    {
                        return (new PowSymbol(log.Left, new MultDivManySymbol(others.ToArray()))).Simplify();
                    }
                    else if (others.Count == 1)
                    {
                        return (new PowSymbol(log.Left, others[0])).Simplify();
                    }
                }
            }

            return new ExpSymbol(l);
        }
    }
}
