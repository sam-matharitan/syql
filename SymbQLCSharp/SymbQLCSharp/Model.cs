using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public delegate Model ModelBuilder();

    public abstract class Model
    {
        public static Model[] ModelDataFileWhole(Worker worker, DataContext dataContext, ModelBuilder builder, int top, int outof, int batch, Func<Model, Model, bool> orderbyFirstIsGreater)
        {
            double[][] allRows = dataContext.AllRows();

            ConcurrentBag<int> bag = new ConcurrentBag<int>();

            Model[] ranked = new Model[top];
            Parallel.For(0, outof, new ParallelOptions { MaxDegreeOfParallelism = 1 }, (i) =>
            {
                if (worker.Token.IsCancellationRequested)
                {
                    return;
                }

                //Task task = Task.Factory.StartNew(() =>
                //{
                    Model model = null;
                    lock (builder)
                    {
                        model = builder();
                    }
                    ModelEvaluation eval = model.Fit(allRows);
                    model.Evaluation = eval;
                    if (model.Evaluation.IsValid)
                    {
                        lock (ranked)
                        {
                            AddRanked(ranked, model, orderbyFirstIsGreater);

                            bag.Add(i);

                            if (bag.Count % batch == 0)
                            {
                                worker.Update(ranked, bag.Count / batch);
                            }
                        }
                    }
                //});

                //task.Wait(1000);
            });

            worker.Update(ranked, -1);

            return ranked;
        }

        private static void AddRanked(Model[] ranked, Model can, Func<Model, Model, bool> orderbyFirstIsGreater)
        {
            for (int i = 0; i < ranked.Length; i++)
            {
                if (ranked[i] == null)
                {
                    ranked[i] = can;
                }
                else if (orderbyFirstIsGreater(can, ranked[i]))
                {
                    Model temp = ranked[i];
                    ranked[i] = can;

                    for (int j = i + 1; j < ranked.Length; j++)
                    {
                        Model temp2 = ranked[j];
                        ranked[j] = temp;
                        temp = temp2;
                    }

                    return;
                }
            }
        }

        public ChooseParameters Parameters { get; set; }

        public ModelEvaluation Evaluation { get; set; }

        public Model(ChooseParameters parameters)
        {
            Parameters = parameters;
        }

        public abstract ModelEvaluation Fit(double[][] data);

        public abstract Symbol FittedLeft();

        public abstract Symbol FittedRight();
    }
}
