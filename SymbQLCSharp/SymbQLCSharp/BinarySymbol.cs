using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public abstract class BinarySymbol : Symbol
    {
        public static List<Chooser> BinaryChoosers = new List<Chooser>();

        static BinarySymbol()
        {
            BinaryChoosers.Add(AddSymbol.Choose);
            BinaryChoosers.Add(MultSymbol.Choose);
            BinaryChoosers.Add(MinusSymbol.Choose);
            BinaryChoosers.Add(DivSymbol.Choose);
            BinaryChoosers.Add(PowSymbol.Choose);
        }

        public static SymbolConnector Choose(ChooseParameters parameters)
        {
            int op = RNG.Random().Next(BinaryChoosers.Count);

            return BinaryChoosers[op](parameters);
        }

        public Symbol Left { get; set; }
        public Symbol Right { get; set; }

        public BinarySymbol(Symbol left, Symbol right)
        {
            Left = left;
            Right = right;

            Left.Parent = this;
            Right.Parent = this;
        }

        public override int Weight()
        {
            return 1 + Left.Weight() + Right.Weight();
        }

        public override List<VarSymbol> GetVariables()
        {
            List<VarSymbol> vars = new List<VarSymbol>();

            vars.AddRange(Left.GetVariables());
            vars.AddRange(Right.GetVariables());

            return vars;
        }

        public override void Organize()
        {
            Left.Organize();
            Right.Organize();
        }
    }
}
