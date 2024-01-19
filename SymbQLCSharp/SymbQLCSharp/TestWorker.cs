using System;

namespace SymbQLCSharp
{
    public class TestWorker : Worker
    {
        public override Model[] Work()
        {
            /////////////////////////////////////////////////////
            /*  
                MODEL TOP 5 OUT OF 1000
                    &f(y) = LINEAR(&f(x))
                DATA FILE "D:\_DATA\Code\ModelScriptCSharp\ModelScriptCSharp\test.txt" (x, y)
                DESC b AS BINARY
                DESC c AS CONSTANT(INVQUANT)
                DESC f OF v AS &b(v, &c)
                ORDER BY SQUAREDERROR
            */
            /////////////////////////////////////////////////////

            ModelBuilder __model = () =>
            {
                ChooseParameters parameters = new ChooseParameters();
                parameters.VarNames = new string[] { "x", "y" };

                VarSymbol x = new VarSymbol(parameters.IndexOfVar("x"));
                VarSymbol y = new VarSymbol(parameters.IndexOfVar("y"));

                Func<SymbolConnector> _b = () => BinarySymbol.Choose(parameters);
                var b = _b();

                Func<SymbolConnector> _c = () => ConstSymbol.InvQuant(parameters);
                var c = _c();

                Func<SymbolConnector> _f = () =>
                {
                    var __b = _b();
                    var __c = _c();

                    return (Symbol[] ___s) => __b(___s[0].Copy(), __c());
                };
                var f = _f();

                return new LinearModel(parameters, _f()(y), _f()(x));
            };

            var __dataContext = new DataFileContext(@"D:\Code\C#\ModelScriptCSharp\ModelScriptCSharp\test.txt");

            var __ranked = Model.ModelDataFileWhole(this, __dataContext, __model, 5, 1000, 100, (m1, m2) => m1.Evaluation.Error < m2.Evaluation.Error);

            return __ranked;
        }
    }
}
