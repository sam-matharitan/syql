using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public abstract class Worker
    {
        public delegate void OnUpdateDelegate(Object sender, OnUpdateEventArgs e);

        public event OnUpdateDelegate OnUpdate;

        private readonly TaskScheduler _context;

        public CancellationToken Token { get; set; }

        public Worker()
        {
            _context = TaskScheduler.Current;
        }

        public void Update(Model[] models, int batch)
        {
            if (OnUpdate != null)
            {
                OnUpdateEventArgs e = new OnUpdateEventArgs(models, batch);
                OnUpdate(this, e);
            }
        }

        public abstract Model[] Work();
        
        protected Symbol REPEAT(Func<SymbolConnector> func, Symbol countSymbol, params Func<SymbolConnector>[] args)
        {
            ConstSymbol count = (ConstSymbol)countSymbol;
            if (count.Value < 1)
            {
                return func()(args.Select(___a => { return ___a()(); }).ToArray());
            }

            return func()(args.Select(___a => REPEAT(func, (Symbol)(count.Value - 1), args)).ToArray());
        }
        
        protected Symbol SIGN(params Symbol[] symbols)
        {
            return new SignSymbol(symbols[0]);
        }

        protected Symbol EXP(params Symbol[] symbols)
        {
            return new ExpSymbol(symbols[0]);
        }

        protected Symbol LN(params Symbol[] symbols)
        {
            return new LogSymbol(symbols[0]);
        }

        protected Symbol SQRT(params Symbol[] symbols)
        {
            return new SqrtSymbol(symbols[0]);
        }
    }
}
