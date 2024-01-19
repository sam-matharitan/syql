
using System;
using System.Collections.Generic;

namespace SymbQLCSharp
{
    public class Worker1550982469 : Worker
    {
        public override Model[] Work()
        {
            ModelBuilder __model = () =>
            {
                ChooseParameters parameters = new ChooseParameters();
                parameters.VarNames = new string[] { "P", "N", "X", "Y" };

                Symbol P = new VarSymbol(parameters.IndexOfVar("P"));
                Symbol N = new VarSymbol(parameters.IndexOfVar("N"));
                Symbol X = new VarSymbol(parameters.IndexOfVar("X"));
                Symbol Y = new VarSymbol(parameters.IndexOfVar("Y"));



                Func<SymbolConnector> _a = () => {
                    List<Chooser> __ch = new List<Chooser>();
                    __ch.AddRange(BinarySymbol.BinaryChoosers);
                    return Symbol.Choose(parameters, __ch.ToArray());
                };
                ;
                var a = _a();

                Func<SymbolConnector> _nDelta = () => {
                    List<Chooser> __ch = new List<Chooser>();
                    __ch.Add(ConstSymbol.BuildChooser(-1, 1, 1));
                    return Symbol.Choose(parameters, __ch.ToArray());
                };
                ;
                var nDelta = _nDelta();

                Func<SymbolConnector> _nMult = () => {
                    List<Chooser> __ch = new List<Chooser>();
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)1);
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)12);
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)4);
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)2);
                    return Symbol.Choose(parameters, __ch.ToArray());
                };
                ;
                var nMult = _nMult();

                Func<SymbolConnector> _nValue = () => {
                    List<Chooser> __ch = new List<Chooser>();
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (_nDelta()() + N.Copy()) * _nMult()());
                    return Symbol.Choose(parameters, __ch.ToArray());
                };
                ;
                var nValue = _nValue();

                Func<SymbolConnector> _pFact = () => {
                    List<Chooser> __ch = new List<Chooser>();
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)1);
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)12);
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)4);
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)2);
                    return Symbol.Choose(parameters, __ch.ToArray());
                };
                ;
                var pFact = _pFact();

                Func<SymbolConnector> _pValue = () => {
                    List<Chooser> __ch = new List<Chooser>();
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => P.Copy() / _pFact()());
                    return Symbol.Choose(parameters, __ch.ToArray());
                };
                ;
                var pValue = _pValue();

                Func<SymbolConnector> _D = () => {
                    List<Chooser> __ch = new List<Chooser>();
                    __ch.Add(ConstSymbol.BuildChooser(-0.01, 0.01, .01));
                    return Symbol.Choose(parameters, __ch.ToArray());
                };
                ;
                var D = _D();

                Func<SymbolConnector> _M = () => {
                    List<Chooser> __ch = new List<Chooser>();
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)1);
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)12);
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)4);
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)2);
                    return Symbol.Choose(parameters, __ch.ToArray());
                };
                ;
                var M = _M();

                Func<SymbolConnector> _V = () => {
                    List<Chooser> __ch = new List<Chooser>();
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (X.Copy() + _D()()) * _M()());
                    return Symbol.Choose(parameters, __ch.ToArray());
                };
                ;
                var V = _V();

                Func<SymbolConnector> _i = () => {
                    List<Chooser> __ch = new List<Chooser>();
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => X.Copy());
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)1 - X.Copy());
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)1 - (Symbol)1 / ((Symbol)1 + X.Copy()));
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)100 * X.Copy());
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)10 * X.Copy());
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => _V()());
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)1 - _V()());
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)1 - (Symbol)1 / ((Symbol)1 + _V()()));
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)100 * _V()());
                    __ch.Add((ChooseParameters p) => (Symbol[] s) => (Symbol)10 * _V()());
                    return Symbol.Choose(parameters, __ch.ToArray());
                };
                ;
                var i = _i();
                return new PureModel(parameters, Y.Copy(), _pValue()() * ((Symbol)1 - ((Symbol)1 / ((Symbol)1 + _i()())) ^ _nValue()()) / _i()());

            };

            var __dataContext = new DataRawContext(new double[][] {
new double[] { 100,20,0.08,1250 }
}
);

            var __ranked = Model.ModelDataFileWhole(this, __dataContext, __model, 5, 1000000, 10000, (m1, m2) => m1.Evaluation.Error < m2.Evaluation.Error);

            return __ranked;
        }
    }
}