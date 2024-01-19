using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class SoftmaxModel : Model
    {
        public Symbol Dependent { get; set; }

        public ArgmaxSoftmaxSymbol Right { get; set; }

        public SoftmaxModel(ChooseParameters parameters, Symbol dependent, params Symbol[] weights) : base(parameters)
        {
            Dependent = dependent;
            Right = new ArgmaxSoftmaxSymbol(weights);
        }

        public override ModelEvaluation Fit(double[][] data)
        {
            double wrong = 0;

            foreach (double[] row in data)
            {
                double cat = Dependent.Eval(row);
                if (double.IsNaN(cat) || double.IsInfinity(cat))
                {
                    return new ModelEvaluation();
                }
                
                double predictedCat = Right.Eval(row);
                if (double.IsNaN(predictedCat) || double.IsInfinity(predictedCat))
                {
                    return new ModelEvaluation();
                }

                if (predictedCat != cat)
                {
                    wrong++;
                }
            }

            return new ModelEvaluation { IsValid = true, Error = wrong / data.Length };
        }

        public override Symbol FittedLeft()
        {
            return Dependent;
        }

        public override Symbol FittedRight()
        {
            return Right;
        }
    }
}
