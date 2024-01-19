using SymbQLCSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestCCSR
{
    public class Worker
    {
        private readonly TaskScheduler _context;

        private CancellationTokenSource _source;

        private string _testScript;

        public Worker(TaskScheduler context)
        {
            _context = context;
        }

        public void Run()
        {
            using (StreamReader streamReader = new StreamReader(""))
            {
                
            }

            _source = new CancellationTokenSource();

            SimpleScriptReader reader = new SimpleScriptReader();
            reader.OnUpdate += Reader_OnUpdate;

            reader.Exec(_testScript, _source.Token);
        }

        private void Reader_OnUpdate(object sender, OnUpdateEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
