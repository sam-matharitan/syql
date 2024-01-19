using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class MultDivManySymbol : Symbol
    {
        public Symbol[] Factors { get; set; }

        public Symbol[] UnderFactors { get; set; } = new Symbol[0];

        public MultDivManySymbol(Symbol[] factors, Symbol[] underFactors = null)
        {
            Factors = factors;
            if (underFactors != null)
            {
                UnderFactors = underFactors;
            }
        }

        public override Symbol Copy()
        {
            return new MultDivManySymbol(Factors.Select(a => a.Copy()).ToArray(), UnderFactors.Select(u => u.Copy()).ToArray());
        }

        public override double Eval(double[] vars)
        {
            return Factors.Select(a => a.Eval(vars)).Aggregate(1.0, (result, a) => result * a) / UnderFactors.Select(u => u.Eval(vars)).Aggregate(1.0, (result, a) => result * a);
        }

        public override string ToExcelString(string[] vars)
        {
            return (Factors.Length > 0 ? string.Join("*", Factors.Select(a =>
            {
                string t = a.ToExcelString(vars);

                if (a is AddSymbol
                    || a is MinusSymbol
                    || a is AddSubManySymbol)
                {
                    t = $"({t})";
                }

                return t;
            })) : "1") + (UnderFactors.Length > 0 ? "/(" + string.Join("*", UnderFactors.Select(a =>
            {
                string t = a.ToExcelString(vars);

                if (a is AddSymbol
                    || a is MinusSymbol
                    || a is AddSubManySymbol)
                {
                    t = $"({t})";
                }

                return t;
            })) + ")" : "");
        }

        public override Symbol Simplify()
        {
            if (GetVariables().Count == 0)
            {
                return new ConstSymbol(Eval(null));
            }

            List<Symbol> newFactors = new List<Symbol>();
            List<Symbol> newUnderFactors = new List<Symbol>();

            double constant = 1;

            foreach (Symbol s in Factors)
            {
                Symbol t = s.Simplify();

                if (t is ConstSymbol)
                {
                    constant *= ((ConstSymbol)t).Value;
                }
                else if (t is MultDivManySymbol)
                {
                    newFactors.AddRange(((MultDivManySymbol)t).Factors);
                    newUnderFactors.AddRange(((MultDivManySymbol)t).UnderFactors);
                }
                else
                {
                    newFactors.Add(t);
                }
            }

            foreach (Symbol s in UnderFactors)
            {
                Symbol t = s.Simplify();

                if (t is ConstSymbol)
                {
                    constant /= ((ConstSymbol)t).Value;
                }
                else if (t is MultDivManySymbol)
                {
                    newFactors.AddRange(((MultDivManySymbol)t).UnderFactors);
                    newUnderFactors.AddRange(((MultDivManySymbol)t).Factors);
                }
                else
                {
                    newUnderFactors.Add(t);
                }
            }

            if (constant != 1)
            {
                newFactors.Add(new ConstSymbol(constant));
            }

            MultDivManySymbol result = new MultDivManySymbol(newFactors.ToArray(), newUnderFactors.ToArray());
            result.Organize();

            int idx = result.GetVariables().Max(v => v.Index);
            string[] vars = new string[idx + 1];
            for (int i = 0; i < vars.Length; i++)
            {
                vars[i] = "V" + i;
            }

            newFactors = result.Factors.ToList();
            newUnderFactors = result.UnderFactors.ToList();

            for (int i = 0; i < newFactors.Count; i++)
            {
                string equA = newFactors[i].ToExcelString(vars);

                for (int j = 0; j < newUnderFactors.Count; j++)
                {
                    string equS = newUnderFactors[j].ToExcelString(vars);

                    if (equA == equS)
                    {
                        newFactors.RemoveAt(i);
                        newUnderFactors.RemoveAt(j);
                        i--;
                        j--;
                        break;
                    }
                }
            }

            if (newFactors.Count == 0 && newUnderFactors.Count == 0)
            {
                return new ConstSymbol(1);
            }

            return new MultDivManySymbol(newFactors.ToArray(), newUnderFactors.ToArray());
        }

        public override int Weight()
        {
            return 1 + Factors.Select(a => a.Weight()).Sum() + UnderFactors.Select(s => s.Weight()).Sum();
        }

        public override List<VarSymbol> GetVariables()
        {
            return Factors.SelectMany(a => a.GetVariables()).Union(UnderFactors.SelectMany(s => s.GetVariables())).ToList();
        }

        public override void Organize()
        {
            foreach (Symbol s in Factors)
            {
                s.Organize();
            }

            foreach (Symbol s in UnderFactors)
            {
                s.Organize();
            }

            List<VarSymbol> vars = GetVariables();
            int idx = vars.Select(v => v.Index).Max();
            double[] data = new double[idx + 1];
            data = data.Select(i => 20.0).ToArray();

            Factors = Factors
                .OrderBy((a) => a.Eval(data)) // approx. order
                .ThenBy((a) => a.Weight()) // Number of total symbols
                .ThenBy((a) => a.GetVariables().GroupBy(v => v.Index).Count()) // Number of unique variables
                .ThenBy((a) => a.GetVariables().GroupBy(v => v.Index).Aggregate("", (result, item) => result + item.Key.ToString())) // Lower index variables first and jointwise
                .ToArray();

            UnderFactors = UnderFactors
                .OrderBy((a) => a.Eval(data)) // approx. order
                .ThenBy((a) => a.Weight()) // Number of total symbols
                .ThenBy((a) => a.GetVariables().GroupBy(v => v.Index).Count()) // Number of unique variables
                .ThenBy((a) => a.GetVariables().GroupBy(v => v.Index).Aggregate("", (result, item) => result + item.Key.ToString())) // Lower index variables first and jointwise
                .ToArray();
        }
    }
}
