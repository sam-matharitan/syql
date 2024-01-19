using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class PureModel : Model
    {
        public Symbol Left { get; set; }

        public Symbol Right { get; set; }

        public PureModel(ChooseParameters parameters, Symbol left, Symbol right) : base(parameters)
        {
            Left = left;
            Right = right;
        }

        public override ModelEvaluation Fit(double[][] data)
        {
            double error = 0;

            foreach (double[] row in data)
            {
                double l = Left.Eval(row);
                if (double.IsNaN(l) || double.IsInfinity(l))
                {
                    return new ModelEvaluation();
                }

                double r = Right.Eval(row);
                if (double.IsNaN(r) || double.IsInfinity(r))
                {
                    return new ModelEvaluation();
                }

                error += (l - r) * (l - r);
            }

            return new ModelEvaluation { IsValid = true, Error = error };
        }

        public override Symbol FittedLeft()
        {
            return Left;
        }

        public override Symbol FittedRight()
        {
            return Right;
        }
    }
}
