using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SymbQLCSharp.WinForms
{
    public partial class MainForm : Form
    {
        private readonly TaskScheduler _context;

        private CancellationTokenSource _source;

        public MainForm()
        {
            InitializeComponent();

            txtBoxScript.Text = @"MODEL TOP 5 OUT OF 100000 BATCH 1000
  Y = &a(&a(1, (1/&a(1,&i))^&n), &i)
DATA RAW (X,Y)
0.01,9.471304531
DESC a AS BINARY
DESC n AS CONSTANT(8,12)
DESC i AS X OR 1-X OR 1-1/(1+X) OR 100*X OR 10*X";

            _context = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            RNG.Reset();

            _source = new CancellationTokenSource();

            listView1.Items.Clear();
            txtBoxStatus.Text = "";

            SimpleScriptReader reader = new SimpleScriptReader();
            reader.OnUpdate += Reader_OnUpdate;

            string script = txtBoxScript.Text;

            try
            {
                Task task = new Task(() =>
                {
                    reader.Exec(script, _source.Token);
                }, _source.Token);

                task.Start();
            }
            catch (Exception ex)
            {
                txtBoxStatus.Text = ex.ToString();
                return;
            }
        }

        private void Reader_OnUpdate(object sender, OnUpdateEventArgs e)
        {
            Task task = new Task(() =>
            {
                try
                {
                    lock (listView1)
                    {
                        listView1.Items.Clear();

                        Model[] result = e.Models;

                        txtBoxStatus.Text = $"(batch {(e.Batch > 0 ? e.Batch.ToString() : "final")}, rows {result.Length})";

                        foreach (Model model in result)
                        {
                            if (model != null)
                            {
                                ListViewItem item = new ListViewItem(new string[]
                                {
                                $"{model.FittedLeft().Simplify().ToExcelString(model.Parameters.VarNames)} = {model.FittedRight().Simplify().ToExcelString(model.Parameters.VarNames)} + ERROR",
                                model.Evaluation.Error.ToString()
                                });

                                listView1.Items.Add(item);
                            }
                        }
                    }

                    listView1.Update();
                    txtBoxStatus.Update();
                }
                finally
                {
                }
                
            });

            task.Start(_context);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_source != null)
            {
                _source.Cancel();
                _source = null;
            }
        }

        private void btnDebugWorker_Click(object sender, EventArgs e)
        {
            Worker1550982469 worker = new Worker1550982469();
            worker.Work();
        }
    }
}
