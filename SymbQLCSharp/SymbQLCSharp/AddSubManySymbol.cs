using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class AddSubManySymbol : Symbol
    {
        public Symbol[] Addends { get; set; }

        public Symbol[] Subends { get; set; } = new Symbol[0];


        public AddSubManySymbol(Symbol[] addends, Symbol[] subends = null)
        {
            Addends = addends;

            if (subends != null)
            {
                Subends = subends;
            }
        }

        public override Symbol Copy()
        {
            return new AddSubManySymbol(Addends.Select(a => a.Copy()).ToArray(), Subends.Select(s => s.Copy()).ToArray());
        }

        public override double Eval(double[] vars)
        {
            return Addends.Select(a => a.Eval(vars)).Sum() - Subends.Select(s => s.Eval(vars)).Sum();
        }

        public override string ToExcelString(string[] vars)
        {
            return string.Join(" + ", Addends.Select(a => a.ToExcelString(vars))) + (Subends.Length > 0 ? " - " + string.Join(" - ", Subends.Select(s => s.ToExcelString(vars))) : "");
        }

        public override Symbol Simplify()
        {
            if (GetVariables().Count == 0)
            {
                return new ConstSymbol(Eval(null));
            }

            List<Symbol> newAddends = new List<Symbol>();
            List<Symbol> newSubends = new List<Symbol>();

            double constant = 0;

            foreach (Symbol s in Addends)
            {
                Symbol t = s.Simplify();

                if (t is NegSymbol)
                {
                    newSubends.Add(t);
                }
                else if (t is ConstSymbol)
                {
                    constant += ((ConstSymbol)t).Value;
                }
                else if (t is AddSubManySymbol)
                {
                    newAddends.AddRange(((AddSubManySymbol)t).Addends);
                    newSubends.AddRange(((AddSubManySymbol)t).Subends);
                }
                else
                {
                    newAddends.Add(t);
                }
            }

            foreach (Symbol s in Subends)
            {
                Symbol t = s.Simplify();

                if (t is NegSymbol)
                {
                    newAddends.Add(t);
                }
                else if (t is ConstSymbol)
                {
                    constant -= ((ConstSymbol)t).Value;
                }
                else if (t is AddSubManySymbol)
                {
                    newAddends.AddRange(((AddSubManySymbol)t).Subends);
                    newSubends.AddRange(((AddSubManySymbol)t).Addends);
                }
                else
                {
                    newSubends.Add(t);
                }
            }

            if (constant != 0)
            {
                newAddends.Add(new ConstSymbol(constant));
            }

            AddSubManySymbol result = new AddSubManySymbol(newAddends.ToArray(), newSubends.ToArray());
            result.Organize();

            int idx = result.GetVariables().Max(v => v.Index);
            string[] vars = new string[idx + 1];
            for (int i = 0; i < vars.Length; i++)
            {
                vars[i] = "V" + i;
            }

            newAddends = result.Addends.ToList();
            newSubends = result.Subends.ToList();

            for (int i = 0; i < newAddends.Count; i++)
            {
                string equA = newAddends[i].ToExcelString(vars);

                for (int j = 0; j < newSubends.Count; j++)
                {
                    string equS = newSubends[j].ToExcelString(vars);

                    if (equA == equS)
                    {
                        newAddends.RemoveAt(i);
                        newSubends.RemoveAt(j);
                        i--;
                        j--;
                    }
                }
            }

            if (newAddends.Count == 0 && newSubends.Count == 0)
            {
                return new ConstSymbol(0);
            }

            return new AddSubManySymbol(newAddends.ToArray(), newSubends.ToArray());
        }

        public override int Weight()
        {
            return 1 + Addends.Select(a => a.Weight()).Sum() + Subends.Select(s => s.Weight()).Sum();
        }

        public override List<VarSymbol> GetVariables()
        {
            return Addends.SelectMany(a => a.GetVariables()).Union(Subends.SelectMany(s => s.GetVariables())).ToList();
        }

        public override void Organize()
        {
            foreach (Symbol s in Addends)
            {
                s.Organize();
            }

            foreach (Symbol s in Subends)
            {
                s.Organize();
            }

            List<VarSymbol> vars = GetVariables();
            int idx = vars.Select(v => v.Index).Max();
            double[] data = new double[idx + 1];
            data = data.Select(i => 20.0).ToArray();

            Addends = Addends
                .OrderBy((a) => a.Eval(data)) // approx. order
                .ThenBy((a) => a.Weight()) // Number of total symbols
                .ThenBy((a) => a.GetVariables().GroupBy(v => v.Index).Count()) // Number of unique variables
                .ThenBy((a) => a.GetVariables().GroupBy(v => v.Index).Aggregate("", (result, item) => result + item.Key.ToString())) // Lower index variables first and jointwise
                .ToArray();

            Subends = Subends
                .OrderBy((a) => a.Eval(data)) // approx. order
                .ThenBy((a) => a.Weight()) // Number of total symbols
                .ThenBy((a) => a.GetVariables().GroupBy(v => v.Index).Count()) // Number of unique variables
                .ThenBy((a) => a.GetVariables().GroupBy(v => v.Index).Aggregate("", (result, item) => result + item.Key.ToString())) // Lower index variables first and jointwise
                .ToArray();
        }
    }
}
