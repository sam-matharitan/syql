using SymbQLCSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelScriptCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleScriptReader reader = new SimpleScriptReader();
            string script = File.ReadAllText(@"D:\Code\C#\ModelScriptCSharp\ModelScriptCSharp\test.script");
            Model[] result = reader.Exec(script, new System.Threading.CancellationToken());

            //Worker worker = new TestWorker();
            //Model[] result = worker.Work();

            foreach (Model model in result)
            {
                Console.WriteLine(string.Format("{2}\t{0} = {1}",
                    model.FittedLeft().ToExcelString(model.Parameters.VarNames),
                    model.FittedRight().ToExcelString(model.Parameters.VarNames),
                    model.Evaluation.Error
                ));
            }

            Console.Read();
        }
    }
}
