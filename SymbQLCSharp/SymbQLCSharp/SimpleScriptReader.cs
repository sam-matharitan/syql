using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SymbQLCSharp
{
    public class SimpleScriptReader
    {
        public delegate void OnUpdateDelegate(Object sender, OnUpdateEventArgs e);

        public event OnUpdateDelegate OnUpdate;

        private readonly TaskScheduler _context;

        public CancellationToken Token { get; set; }

        public SimpleScriptReader()
        {
            _context = TaskScheduler.Current;
        }

        public Model[] Exec(string script, CancellationToken token)
        {
            Token = token;

            Worker worker = BuildCSWorker(script);
            worker.Token = token;
            worker.OnUpdate += Worker_OnUpdate;

            return worker.Work();
        }

        private void Worker_OnUpdate(object sender, OnUpdateEventArgs e)
        {
            if (OnUpdate != null)
            {
                OnUpdate(sender, e);
            }
        }

        private Worker BuildCSWorker(string script)
        {
            string csClassName = $"Worker{Math.Abs(Guid.NewGuid().GetHashCode())}";

            string[] lines = script.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string top = "1", outof = "1", batch = "1";
            string modelString = "";
            string dataFilename = "";
            string[] vars = new string[0];

            int descStart = 3;

            string line0pattern = @"^MODEL TOP (?<top>\d+) OUT OF (?<outof>\d+) BATCH (?<batch>\d+)$";
            Regex line0regex = new Regex(line0pattern);
            if (line0regex.IsMatch(lines[0]))
            {
                Match m = line0regex.Match(lines[0]);
                top = m.Groups["top"].Value;
                outof = m.Groups["outof"].Value;
                batch = m.Groups["batch"].Value;
            }
            else
            {
                throw new Exception("Line 0 doesn't match.");
            }

            modelString = lines[1].Trim().Replace(" ", "").Replace("\t", "");

            string dataContext = "";

            string line2DataFilepattern = @"^DATA FILE ""(?<file>.+)"" \((?<vars>.+)\)$";
            string line2DataRawpattern = @"^DATA RAW \((?<vars>.+)\)$";
            Regex line2regex = new Regex(line2DataFilepattern);
            Regex line2Rawregex = new Regex(line2DataRawpattern);
            if (line2regex.IsMatch(lines[2]))
            {
                Match m = line2regex.Match(lines[2]);
                dataFilename = m.Groups["file"].Value;
                vars = m.Groups["vars"].Value.Replace(" ", "").Split(new char[] { ',' });

                dataContext = $"DataFileContext(@\"{dataFilename}\")";
            }
            else if (line2Rawregex.IsMatch(lines[2]))
            {
                Match m = line2Rawregex.Match(lines[2]);
                vars = m.Groups["vars"].Value.Replace(" ", "").Split(new char[] { ',' });

                StringBuilder sbArray = new StringBuilder();
                sbArray.AppendLine("new double[][] {");
                for (;descStart < lines.Length && !lines[descStart].StartsWith("DESC"); descStart ++)
                {
                    if (descStart + 1 < lines.Length && lines[descStart+1].StartsWith("DESC"))
                    {
                        sbArray.AppendLine($"new double[] {{ {lines[descStart]} }}");
                    }
                    else
                    {
                        sbArray.AppendLine($"new double[] {{ {lines[descStart]} }},");
                    }
                }
                sbArray.AppendLine("}");

                dataContext = $"DataRawContext({sbArray.ToString()})";
            }
            else
            {
                throw new Exception("Line 2 doesn't match.");
            }

            StringBuilder sbModel = new StringBuilder();
            sbModel.AppendLine("ChooseParameters parameters = new ChooseParameters();");

            string varNames = $"parameters.VarNames = new string[] {{ \"{string.Join("\",\"", vars)}\" }};";
            sbModel.AppendLine(varNames);
            sbModel.AppendLine();

            foreach (string v in vars)
            {
                sbModel.AppendLine($"Symbol {v} = new VarSymbol(parameters.IndexOfVar(\"{v}\"));");
            }

            sbModel.AppendLine();

            List<string> descs = new List<string>();
            List<string> exprs = new List<string>(vars);
            List<string> refs = new List<string>();
            List<string> excl = new List<string>();

            Func<string, string[], string> prepare = (pre, inputs) =>
            {
                Regex rep1 = new Regex($"((?<name>(?!\\b\\d+\\b|\\b{string.Join("\\b|\\b", excl)}\\b)\\b\\w+\\b)(?!\\())");
                Regex rep2 = new Regex(@"&(?<name>(?!\d+)\b\w+\b)");
                Regex rep6 = new Regex(@"@(?<name>(?!\d+)\b\w+\b\(\))");
                Regex rep5 = new Regex(@"(?<number>(\d*\.\d+|\b\d+)\b)");
                Regex rep4 = new Regex($"(?<name>\\b{string.Join("\\b|\\b", exprs)}\\b)");
                Regex rep3 = new Regex($"(?<name>\\b{string.Join("\\b|\\b", inputs)}\\b)");

                pre = rep1.Replace(pre, (m) =>
                {
                    return $"{m.Groups["name"].Value}()";
                });

                pre = rep2.Replace(pre, (m) =>
                {
                    if (inputs.Length > 0)
                    {
                        string name = m.Groups["name"].Value;
                        refs.Add(name);
                        return $"_{refs.Count(x => x.Equals(name))}{name}";
                    }
                    else
                        return $"_{m.Groups["name"].Value}()";
                });

                pre = rep6.Replace(pre, (m) =>
                {
                    return $"_{m.Groups["name"].Value.Replace("()", "")}";
                });

                pre = rep5.Replace(pre, (m) =>
                {
                    return $"~0~{m.Groups["number"].Value}";
                });

                pre = rep4.Replace(pre, (m) =>
                {
                    return $"{m.Groups["name"].Value}~1~";
                });

                if (inputs.Length > 0)
                {
                    pre = rep3.Replace(pre, (m) =>
                    {
                        return $"___s[{Array.IndexOf(inputs, m.Groups["name"].Value)}].Copy()";
                    });
                }

                pre = pre.Replace("~0~", "(Symbol)").Replace("~1~", ".Copy()");

                return pre;
            };

            Func<string, string[], string> descToCS = (d, inputs) =>
            {
                excl.Clear();
                excl.AddRange(exprs);
                excl.AddRange(inputs);

                refs.Clear();

                

                string[] ors = d.Split(new string[] { " OR " }, StringSplitOptions.None);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("List<Chooser> __ch = new List<Chooser>();");
                foreach (string or in ors)
                {
                    if (or == "ANY")
                    {
                        sb.AppendLine("__ch.AddRange(Symbol.Choosers);");
                    }
                    else if (or == "UNARY")
                    {
                        sb.AppendLine("__ch.AddRange(UnarySymbol.UnaryChoosers);");
                    }
                    else if (or == "BINARY")
                    {
                        sb.AppendLine("__ch.AddRange(BinarySymbol.BinaryChoosers);");
                    }
                    else if (or == "CONSTANT(INVQUANT)")
                    {
                        sb.AppendLine("__ch.Add(ConstSymbol.InvQuant);");
                    }
                    else if (or.StartsWith("CONSTANT"))
                    {
                        sb.AppendLine($"__ch.Add(ConstSymbol.BuildChooser{or.Replace("CONSTANT", "")});");
                    }
                    else if (or == "VARIABLE")
                    {
                        sb.AppendLine("__ch.Add(VarSymbol.Choose);");
                    }
                    else if (or.StartsWith("VARIABLE"))
                    {
                        string varStr = or.Replace("VARIABLE", "").Replace("(", "").Replace(")", "");
                        Regex varRegex = new Regex(@"(?<name>\w+)");
                        varStr = varRegex.Replace(varStr, (m) =>
                        {
                            return $"parameters.IndexOfVar(\"{m.Groups["name"].Value}\")";
                        });

                        sb.AppendLine($"__ch.Add(VarSymbol.BuildChooser(new int[] {{ {varStr} }}));");
                    }
                    else if (or == "SIGN")
                    {
                        sb.AppendLine("__ch.Add(SignSymbol.Choose);");
                    }
                    else if (or == "EXP")
                    {
                        sb.AppendLine("__ch.Add(ExpSymbol.Choose);");
                    }
                    else if (or == "LN")
                    {
                        sb.AppendLine("__ch.Add(LogSymbol.Choose);");
                    }
                    else if (or == "SQRT")
                    {
                        sb.AppendLine("__ch.Add(SqrtSymbol.Choose);");
                    }
                    else if (or == "SQUARE")
                    {
                        sb.AppendLine("__ch.Add(SquareSymbol.Choose);");
                    }
                    else if (or == "NEG")
                    {
                        sb.AppendLine("__ch.Add(NegSymbol.Choose);");
                    }
                    else if (or == "ADD")
                    {
                        sb.AppendLine("__ch.Add(AddSymbol.Choose);");
                    }
                    else if (or == "MINUS")
                    {
                        sb.AppendLine("__ch.Add(MinusSymbol.Choose);");
                    }
                    else if (or == "MULT")
                    {
                        sb.AppendLine("__ch.Add(MultSymbol.Choose);");
                    }
                    else if (or == "DIV")
                    {
                        sb.AppendLine("__ch.Add(DivSymbol.Choose);");
                    }
                    else if (or == "POW")
                    {
                        sb.AppendLine("__ch.Add(PowSymbol.Choose);");
                    }
                    else if (or == "SIN")
                    {
                        sb.AppendLine("__ch.Add(SinSymbol.Choose);");
                    }
                    else if (or == "COS")
                    {
                        sb.AppendLine("__ch.Add(CosSymbol.Choose);");
                    }
                    else if (inputs.Length == 0)
                    {
                        sb.AppendLine($"__ch.Add((ChooseParameters p) => (Symbol[] s) => {prepare(or, inputs)});");
                    }
                    else
                    {
                        string conv = prepare(or, inputs);
                        StringBuilder sb2 = new StringBuilder();
                        sb2.AppendLine();
                        sb2.AppendLine("{");
                        List<string> cts = new List<string>();
                        foreach (string r in refs)
                        {
                            cts.Add(r);
                            sb2.AppendLine($"var _{cts.Count(x => x.Equals(r))}{r} = _{r}();");
                        }

                        sb2.AppendLine($"return (Symbol[] ___s) => {conv};");
                        sb2.AppendLine("}");

                        sb.AppendLine($"__ch.Add((ChooseParameters p) => {sb2.ToString()});");
                    }
                }
                sb.AppendLine("return Symbol.Choose(parameters, __ch.ToArray());");
                sb.AppendLine("};");

                return sb.ToString();
            };

            Regex descRegex = new Regex(@"^DESC (?<name>\w+) AS (?<desc>.+)$");
            Regex descOfRegex = new Regex(@"^DESC (?<name>\w+) OF (?<ins>.+) AS (?<desc>.+)$");

            for (int i = 3; i < lines.Length; i ++)
            {
                sbModel.AppendLine();
                if (descOfRegex.IsMatch(lines[i]))
                {
                    Match m = descOfRegex.Match(lines[i]);
                    string name = m.Groups["name"].Value;
                    string[] ins = m.Groups["ins"].Value.Replace(" ", "").Split(new char[] { ',' });
                    descs.Add(name);
                    sbModel.AppendLine($"Func<SymbolConnector> _{name} = () => {descToCS(m.Groups["desc"].Value, ins)};");
                    sbModel.AppendLine($"var {name} = _{name}();");

                }
                else if (descRegex.IsMatch(lines[i]))
                {
                    Match m = descRegex.Match(lines[i]);
                    string name = m.Groups["name"].Value;
                    descs.Add(name);
                    sbModel.AppendLine($"Func<SymbolConnector> _{name} = () => {descToCS(m.Groups["desc"].Value, new string[0])};");
                    sbModel.AppendLine($"var {name} = _{name}();");
                }
            }

            string dep = "";
            string[] inds = new string[0];
            string barrier = "";

            string modelStringLinearPattern = @"^(?<dep>.+)=LINEAR\((?<inds>.+)\)$";
            Regex modelStringLinearRegex = new Regex(modelStringLinearPattern);
            Regex modelStringBarrierRegex = new Regex(@"^(?<dep>.+)=BARRIER\((?<barrier>\d+|\d+\.\d+|\.\d+),(?<inds>.+)\)$");
            Regex modelStringSoftmaxRegex = new Regex(@"^(?<dep>.+)=SOFTMAX\((?<inds>.+)\)$");
            if (modelStringLinearRegex.IsMatch(modelString))
            {
                Match m = modelStringLinearRegex.Match(modelString);
                dep = m.Groups["dep"].Value;
                inds = m.Groups["inds"].Value.Split(new char[] { '|' });

                sbModel.AppendLine($"return new LinearModel(parameters, {prepare(dep, new string[0])}, {string.Join(",", inds.Select(i => prepare(i, new string[0])))});");
            }
            else if (modelStringBarrierRegex.IsMatch(modelString))
            {
                Match m = modelStringBarrierRegex.Match(modelString);
                dep = m.Groups["dep"].Value;
                inds = m.Groups["inds"].Value.Split(new char[] { '|' });
                barrier = m.Groups["barrier"].Value;

                sbModel.AppendLine($"return new BarrierModel(parameters, {barrier}, {prepare(dep, new string[0])}, {string.Join(",", inds.Select(i => prepare(i, new string[0])))});");
            }
            else if (modelStringSoftmaxRegex.IsMatch(modelString))
            {
                Match m = modelStringSoftmaxRegex.Match(modelString);
                dep = m.Groups["dep"].Value;
                inds = m.Groups["inds"].Value.Split(new char[] { '|' });

                sbModel.AppendLine($"return new SoftmaxModel(parameters, {prepare(dep, new string[0])}, {string.Join(",", inds.Select(i => prepare(i, new string[0])))});");
            }
            else
            {
                Regex modelStringPureRegex = new Regex(@"^(?<left>.+)=(?<right>.+)");
                if (modelStringPureRegex.IsMatch(modelString))
                {
                    Match m = modelStringPureRegex.Match(modelString);
                    string left = m.Groups["left"].Value;
                    string right = m.Groups["right"].Value;

                    sbModel.AppendLine($"return new PureModel(parameters, {prepare(left, new string[0])}, {prepare(right, new string[0])});");
                }
                else
                {
                    throw new Exception("Model string invalid.");
                }
            }

            string orderby = @"(m1, m2) => m1.Evaluation.Error < m2.Evaluation.Error";

            string modelCall = $"ModelDataFileWhole(this, __dataContext, __model, {top}, {outof}, {batch}, {orderby})";

            string csClass = string.Format(_worker_class_frame, csClassName, sbModel.ToString(), modelCall, dataContext);

            Assembly asm = GetCompiledAssembly(csClass);
            var t = asm.GetType("SymbQLCSharp." + csClassName);
            Worker worker = (Worker)Activator.CreateInstance(t);
            
            return worker;
        }

        private Assembly GetCompiledAssembly(string csClass)
        {
            var refs = AppDomain.CurrentDomain.GetAssemblies();
            var refFiles = refs.Where(a => !a.IsDynamic).Select(a => a.Location).ToArray();
            var cSharp = new Microsoft.CSharp.CSharpCodeProvider();
            var compileParams = new System.CodeDom.Compiler.CompilerParameters(refFiles);
            compileParams.GenerateInMemory = true;
            compileParams.GenerateExecutable = false;

            var compilerResult = cSharp.CompileAssemblyFromSource(compileParams, csClass);
            var asm = compilerResult.CompiledAssembly;

            return asm;
        }

        private string _worker_class_frame = @"
using System;
using System.Collections.Generic;

namespace SymbQLCSharp
{{
    public class {0} : Worker
    {{
        public override Model[] Work()
        {{
            ModelBuilder __model = () =>
            {{
                {1}
            }};

            var __dataContext = new {3};

            var __ranked = Model.{2};

            return __ranked;
        }}
    }}
}}";
    }
}
