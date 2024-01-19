using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class LinearModel : Model
    {
        public Symbol Dependent { get; set; }

        public Symbol[] Independents { get; set; }

        public double[] Coefficients { get; set; }

        public LinearModel(ChooseParameters parameters, Symbol dependent, params Symbol[] independents) : base(parameters)
        {
            Dependent = dependent;
            Independents = independents;
        }

        public override ModelEvaluation Fit(double[][] rows)
        {
            double[] yColumn = new double[rows.Length];
            double[][] xColumns = new double[Independents.Length + 1][];

            for (int i = 0; i < Independents.Length + 1; i++)
            {
                xColumns[i] = new double[rows.Length];
            }

            for (int r = 0; r < rows.Length; r++)
            {
                double[] row = rows[r];

                yColumn[r] = Dependent.Eval(row);
                if (double.IsNaN(yColumn[r]) || double.IsInfinity(yColumn[r]))
                {
                    return new ModelEvaluation();
                }

                xColumns[0][r] = 1;
                for (int i = 1; i < Independents.Length + 1; i++)
                {
                    xColumns[i][r] = Independents[i-1].Eval(row);
                    if (double.IsNaN(xColumns[i][r]) || double.IsInfinity(xColumns[i][r]))
                    {
                        return new ModelEvaluation();
                    }
                }
            }

            Matrix<double> X = Matrix<double>.Build.Dense(yColumn.Length, xColumns.Length, (i, j) => xColumns[j][i]); //Matrix<double>.Build.DenseOfColumnArrays(xColumns);
            Vector<double> y = Vector<double>.Build.DenseOfArray(yColumn);

            try
            {
                Vector<double> par = MultipleRegression.DirectMethod(X, y);

                Coefficients = par.ToArray();

                double error = (y - X * par).Sum((x) => x * x);

                return new ModelEvaluation() { IsValid = true, Error = error };
            }
            catch (Exception)
            {
                return new ModelEvaluation();
            }
        }

        public override Symbol FittedLeft()
        {
            return Dependent;
        }

        public override Symbol FittedRight()
        {
            return _FittedRight(0);
        }

        private Symbol _FittedRight(int coef)
        {
            if (coef == 0)
            {
                if (Coefficients.Length > 1)
                {
                    return new AddSymbol(new ConstSymbol(Coefficients[coef]), _FittedRight(coef + 1));
                }
                else
                {
                    return new ConstSymbol(Coefficients[coef]);
                }
            }
            else if (coef == Coefficients.Length - 1)
            {
                return new MultSymbol(new ConstSymbol(Coefficients[coef]), Independents[coef - 1].Copy());
            }
            else
            {
                return new AddSymbol(new MultSymbol(new ConstSymbol(Coefficients[coef]), Independents[coef - 1].Copy()), _FittedRight(coef + 1));
            }
        }
    }
}
