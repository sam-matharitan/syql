using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public abstract class UnarySymbol : Symbol
    {
        public static List<Chooser> UnaryChoosers = new List<Chooser>();

        static UnarySymbol()
        {
            UnaryChoosers.Add(LogSymbol.Choose);
            UnaryChoosers.Add(ExpSymbol.Choose);
            UnaryChoosers.Add(SqrtSymbol.Choose);
            UnaryChoosers.Add(NegSymbol.Choose);
            UnaryChoosers.Add(SquareSymbol.Choose);
            UnaryChoosers.Add(SignSymbol.Choose);
            UnaryChoosers.Add(SinSymbol.Choose);
            UnaryChoosers.Add(CosSymbol.Choose);
        }

        public static SymbolConnector Choose(ChooseParameters parameters)
        {
            int op = RNG.Random().Next(UnaryChoosers.Count);

            return UnaryChoosers[op](parameters);
        }

        public Symbol Left { get; set; }

        public UnarySymbol(Symbol left)
        {
            Left = left;

            Left.Parent = this;
        }

        public override int Weight()
        {
            return 1 + Left.Weight();
        }

        public override List<VarSymbol> GetVariables()
        {
            return Left.GetVariables();
        }

        public override void Organize()
        {
            Left.Organize();
        }
    }
}
