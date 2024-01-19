using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class MinusSymbol : BinarySymbol
    {
        public static new SymbolConnector Choose(ChooseParameters parameters)
        {
            return (Symbol[] symbols) =>
            {
                return new MinusSymbol(symbols[0], symbols[1]);
            };
        }

        public MinusSymbol(Symbol left, Symbol right) : base(left, right)
        {

        }

        public override double Eval(double[] vars)
        {
            double l = Left.Eval(vars);
            double r = Right.Eval(vars);

            return l - r;
        }

        public override string ToExcelString(string[] vars)
        {
            string l = Left.ToExcelString(vars);
            string r = Right.ToExcelString(vars);

            if (false
                || Right is AddSymbol
                || Right is MinusSymbol)
            {
                r = $"({r})";
            }

            return $"{l}-{r}";
        }

        public override Symbol Copy()
        {
            return new MinusSymbol(Left.Copy(), Right.Copy());
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
                syms.AddRange(((AddSubManySymbol)r).Subends);
                symsSub.AddRange(((AddSubManySymbol)r).Addends);
            }
            else
            {
                symsSub.Add(r);
            }

            return (new AddSubManySymbol(syms.ToArray(), symsSub.ToArray())).Simplify();
        }
    }
}
