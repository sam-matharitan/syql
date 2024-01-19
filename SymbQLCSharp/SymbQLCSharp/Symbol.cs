using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public delegate Symbol SymbolConnector(params Symbol[] symbols);

    public delegate SymbolConnector Chooser(ChooseParameters parameters);

    public abstract class Symbol
    {
        public static List<Chooser> Choosers = new List<Chooser>();

        static Symbol()
        {
            Choosers.Add(AddSymbol.Choose);
            Choosers.Add(MinusSymbol.Choose);
            Choosers.Add(MultSymbol.Choose);
            Choosers.Add(ConstSymbol.Choose);
            Choosers.Add(VarSymbol.Choose);
            Choosers.Add(DivSymbol.Choose);
            Choosers.Add(PowSymbol.Choose);
            Choosers.Add(LogSymbol.Choose);
            Choosers.Add(ExpSymbol.Choose);
            Choosers.Add(SqrtSymbol.Choose);
            Choosers.Add(NegSymbol.Choose);
            Choosers.Add(SquareSymbol.Choose);
            Choosers.Add(SignSymbol.Choose);
        }

        public static SymbolConnector Choose(ChooseParameters parameters, bool dummy)
        {
            int op = RNG.Random().Next(Choosers.Count);

            return Choosers[op](parameters);
        }

        public static SymbolConnector Choose(ChooseParameters parameters, params Chooser[] choosers)
        {
            if (choosers.Length == 0)
            {
                return Choose(parameters, false);
            }

            int op = RNG.Random().Next(choosers.Length);
            return choosers[op](parameters);
        }

        public Symbol Parent { get; set; } = null;

        public abstract double Eval(double[] vars);

        public abstract string ToExcelString(string[] vars);

        public abstract Symbol Copy();

        public abstract Symbol Simplify();

        public abstract int Weight();

        public abstract List<VarSymbol> GetVariables();

        public abstract void Organize();

        public static Symbol operator *(Symbol l, Symbol r)
        {
            return new MultSymbol(l, r);
        }

        public static Symbol operator *(double l, Symbol r)
        {
            return new MultSymbol(new ConstSymbol(l), r);
        }

        public static Symbol operator *(Symbol l, double r)
        {
            return new MultSymbol(l, new ConstSymbol(r));
        }

        public static Symbol operator +(Symbol l, Symbol r)
        {
            return new AddSymbol(l, r);
        }

        public static Symbol operator +(double l, Symbol r)
        {
            return new AddSymbol(new ConstSymbol(l), r);
        }

        public static Symbol operator +(Symbol l, double r)
        {
            return new AddSymbol(l, new ConstSymbol(r));
        }

        public static Symbol operator -(Symbol l, Symbol r)
        {
            return new MinusSymbol(l, r);
        }

        public static Symbol operator -(double l, Symbol r)
        {
            return new MinusSymbol(new ConstSymbol(l), r);
        }

        public static Symbol operator -(Symbol l, double r)
        {
            return new MinusSymbol(l, new ConstSymbol(r));
        }

        public static Symbol operator -(Symbol l)
        {
            return new NegSymbol(l);
        }

        public static Symbol operator /(Symbol l, Symbol r)
        {
            return new DivSymbol(l, r);
        }

        public static Symbol operator /(double l, Symbol r)
        {
            return new DivSymbol(new ConstSymbol(l), r);
        }

        public static Symbol operator /(Symbol l, double r)
        {
            return new DivSymbol(l, new ConstSymbol(r));
        }
        
        public static Symbol operator ^(Symbol l, Symbol r)
        {
            return new PowSymbol(l, r);
        }

        public static Symbol operator ^(double l, Symbol r)
        {
            return new PowSymbol((Symbol)l, r);
        }

        public static Symbol operator ^(Symbol l, double r)
        {
            return new PowSymbol(l, (Symbol)r);
        }

        public static explicit operator Symbol(double b) => new ConstSymbol(b);
        public static explicit operator Symbol(int b) => new ConstSymbol(b);
    }
}
