using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class ConstSymbol : Symbol
    {
        public static SymbolConnector Choose(ChooseParameters parameters)
        {
            double c = (double)RNG.Random().Next(parameters.ConstMinValue, parameters.ConstMaxValue);

            return (Symbol[] symbols) =>
            {
                return new ConstSymbol(c);
            };
        }

        public static SymbolConnector InvQuant(ChooseParameters parameters)
        {
            double c = 1 / RNG.Random().NextDouble() - 1;

            return (Symbol[] symbols) =>
            {
                return new ConstSymbol(c);
            };
        }

        public static Chooser BuildChooser(int min, int max)
        {
            return (ChooseParameters p) =>
            {
                double c = (double)RNG.Random().Next(min, max);

                return (Symbol[] symbols) =>
                {
                    return new ConstSymbol(c);
                };
            };
        }

        public static Chooser BuildChooser(double min, double max, double step)
        {
            return (ChooseParameters p) =>
            {
                double c = min + step * RNG.Random().Next(0, (int)((max - min)/step));

                return (Symbol[] symbols) =>
                {
                    return new ConstSymbol(c);
                };
            };
        }

        public double Value { get; set; }

        public ConstSymbol()
        {
            Value = RNG.Random().NextDouble();
        }

        public ConstSymbol(double value)
        {
            Value = value;
        }

        public ConstSymbol(int minValue, int maxValue)
        {
            Value = RNG.Random().Next(minValue, maxValue);
        }

        public override double Eval(double[] vars)
        {
            return Value;
        }

        public override string ToExcelString(string[] vars)
        {
            return Value.ToString();
        }

        public override Symbol Copy()
        {
            return new ConstSymbol(Value);
        }

        public override Symbol Simplify()
        {
            return Copy();
        }

        public override int Weight()
        {
            return 1;
        }

        public override List<VarSymbol> GetVariables()
        {
            return new List<VarSymbol>();
        }

        public override void Organize()
        {
            return;
        }
    }
}
