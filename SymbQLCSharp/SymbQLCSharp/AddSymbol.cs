using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class AddSymbol : BinarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new AddSymbol(symbols[0], symbols[1]);
            };
        }

        public AddSymbol(Symbol left, Symbol right) : base(left, right)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);
            double r = Right.Eval(vars);

            return l + r;
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);
            string r = Right.ToExcelString(vars);
            
            return $"{l}+{r}";
        }

        public override Symbol Copy()
        {
            return new AddSymbol(Left.Copy(), Right.Copy());
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
            if (l is AddSubManySymbol)
            {
                syms.AddRange(((AddSubManySymbol)l).Addends);
                symsSub.AddRange(((AddSubManySymbol)l).Subends);
            }
            else
            {
                syms.Add(l);
            }

            if (r is AddSubManySymbol)
            {
                syms.AddRange(((AddSubManySymbol)r).Addends);
                symsSub.AddRange(((AddSubManySymbol)r).Subends);
            }
            else
            {
                syms.Add(r);
            }

            return (new AddSubManySymbol(syms.ToArray(), symsSub.ToArray())).Simplify();
        }
    }
}
