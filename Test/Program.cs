using System;
using System.Diagnostics;
using System.Linq;
using FEA.Models;
using FEA.Plate;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Truess2D_1();
            //Truess2D_2();
            //Truess3D();
            //Frame2D_1();
            //Frame3D_1();
            //Plate_1();

            Console.Write("Press any key to exit.");
            Console.ReadKey();
        }

        static void LoadModel(string content)
        {
            var lines = content.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            
            Model model;
            foreach (var line in lines)
            {
                var parts = line
                    .Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToArray();
                if (parts.Length == 0 || parts[0].StartsWith("$"))
                {
                    continue;
                }

                if (parts.Length < 2)
                {
                    throw new Exception($"Invalid line:\r\n{line}");
                }
                var command = parts.Last();
                var args = parts.TakeLast(parts.Length - 1).ToList();

                switch (command)
                {
                    case "model":
                        model = args[0] switch
                        {
                            "truss" => args[1] switch
                            {
                                "2D" => new ModelTruss2D(),
                                "3D" => new ModelTruss3D(),
                                _ => throw new Exception()
                            },
                            "frame" => args[1] switch
                            {
                                "2D" => new ModelFrame2D(),
                                "3D" => new ModelFrame3D(),
                                _ => throw new Exception()
                            },
                            _ => throw new Exception()
                        };
                        break;
                    case "node":

                        break;
                    case "element":
                        break;
                    case "fix":
                        break;
                }

            }


        }

        static void Truess2D_1()
        {
            var model = new ModelTruss2D();
            model.AddNode("1", 0, 0);
            model.AddNode("2", 1, 0);
            model.AddNode("3", 0, 1);

            model.AddMaterial("1", 100_000);
            model.AddProfileSection("1", 1_000);

            model.AddElement("1", "1", "2", "1", "1");
            model.AddElement("2", "2", "3", "1", "1");
            model.AddElement("3", "1", "3", "1", "1");

            model.AddRestraint("1", true, true);
            model.AddRestraint("2", false, true);

            model.AddLoadCase("D");
            model.AddLoadCase("L");

            model.AddNodalForce("3", "D", 100, 0);

            model.AddNodalForce("3", "L", 100, 0);
            model.AddNodalForce("2", "L", 100, 0);

            model.Build();

            model.RunAnalysis();

            model.PostAnalysis();

            Console.WriteLine($"Truss Forces -----------------------------------");
            foreach (var element in model.LineElements)
            {
                Console.WriteLine($"{element.Name,-5} {element.InternalForce,10:F2} {element.Stress,10:F2} {element.Elongation,15:E3}");
            }
            Console.WriteLine();

            Console.WriteLine($"Reaction Forces -----------------------------------");
            for (var j = 0; j < model.LoadCases.Count; j++)
            {
                Console.WriteLine();
                Console.WriteLine($"Load Case: {model.LoadCases[j].ToString()}");
                foreach (var node in model.Nodes)
                {
                    if (node.IsSupport.All(p => !p))
                    {
                        continue;
                    }
                    Console.WriteLine($"{node.Name,-5} {model.LoadCases[j],-5} {string.Join("\t", node.ReactionForce[j].Select(p => $"{p,10:F2}").ToList())}");
                }
            }
            Console.WriteLine();

            Console.WriteLine($"Resultant Displacements -----------------------------------");
            foreach (var node in model.Nodes)
            {
                if (node.IsSupport.All(p => p))
                {
                    continue;
                }

                for (var j = 0; j < model.LoadCases.Count; j++)
                {
                    Console.WriteLine($"{node.Name,-5} {model.LoadCases[j],-5} {string.Join("\t", node.ResultantDisplacement[j].Select(p => $"{p,6:E3}").ToList())}");
                }
            }
            Console.WriteLine();
            Console.WriteLine("||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
        }

        static void Truess2D_2()
        {
            var model = new ModelTruss2D();

            model.AddNode("1", 0, 0);
            model.AddNode("2", 2, 0);
            model.AddNode("3", 4, 0);
            model.AddNode("4", 6, 0);
            model.AddNode("5", 8, 0);
            model.AddNode("6", 10, 0);
            model.AddNode("7", 12, 0);
            model.AddNode("8", 2, 2);
            model.AddNode("9", 4, 2);
            model.AddNode("10", 6, 2);
            model.AddNode("11", 8, 2);
            model.AddNode("12", 10, 2);

            model.AddMaterial("1", 2.1e11);
            model.AddProfileSection("1", 0.01);

            model.AddElement("1", "1", "2", "1", "1");
            model.AddElement("2", "2", "3", "1", "1");
            model.AddElement("3", "3", "4", "1", "1");
            model.AddElement("4", "4", "5", "1", "1");
            model.AddElement("5", "5", "6", "1", "1");
            model.AddElement("6", "6", "7", "1", "1");
            model.AddElement("7", "1", "8", "1", "1");
            model.AddElement("8", "8", "9", "1", "1");
            model.AddElement("9", "9", "10", "1", "1");
            model.AddElement("10", "10", "11", "1", "1");
            model.AddElement("11", "11", "12", "1", "1");
            model.AddElement("12", "12", "7", "1", "1");
            model.AddElement("13", "2", "8", "1", "1");
            model.AddElement("14", "3", "9", "1", "1");
            model.AddElement("15", "4", "10", "1", "1");
            model.AddElement("16", "5", "11", "1", "1");
            model.AddElement("17", "6", "12", "1", "1");
            model.AddElement("18", "3", "8", "1", "1");
            model.AddElement("19", "4", "9", "1", "1");
            model.AddElement("20", "4", "11", "1", "1");
            model.AddElement("21", "5", "12", "1", "1");

            model.AddRestraint("1", true, true);
            model.AddRestraint("7", false, true);

            model.AddLoadCase("D");

            model.AddNodalForce("3", "D", 0, -10);
            model.AddNodalForce("4", "D", 0, -10);
            model.AddNodalForce("5", "D", 0, -10);

            model.Build();

            model.RunAnalysis();

            model.PostAnalysis();

            Console.WriteLine($"Truss Forces -----------------------------------");
            foreach (var element in model.LineElements)
            {
                Debug.WriteLine($"{element.Name,-5} {element.InternalForce,10:F2} {element.Stress,10:F2} {element.Elongation,15:E3}");
            }
            Console.WriteLine();

            //Console.WriteLine($"Reaction Forces -----------------------------------");
            //foreach (var node in model.Nodes)
            //{
            //    if (node.IsSupport.All(p => !p))
            //    {
            //        continue;
            //    }
            //    Console.WriteLine($"{node.Name,-5} {string.Join("\t", node.ReactionForce.Select(p => $"{p,10:F2}").ToList())}");
            //}
            //Console.WriteLine();

            //Console.WriteLine($"Resultant Displacements -----------------------------------");
            //foreach (var node in model.Nodes)
            //{
            //    if (node.IsSupport.All(p => p))
            //    {
            //        continue;
            //    }
            //    Console.WriteLine($"{node.Name,-5} {string.Join("\t", node.ResultantDisplacement.Select(p => $"{p,6:E3}").ToList())}");
            //}
            Console.WriteLine();
            Console.WriteLine("||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
        }

        static void Truess3D()
        {
            var model = new ModelTruss3D();
            model.AddNode("1", -1, -1, 0);
            model.AddNode("2", -1, 1, 0);
            model.AddNode("3", 1, -1, 0);
            model.AddNode("4", 1, 1, 0);
            model.AddNode("5", 0, 0, 1);

            model.AddMaterial("1", 10_000);
            model.AddProfileSection("1", 1);

            model.AddElement("1", "1", "5", "1", "1");
            model.AddElement("2", "2", "5", "1", "1");
            model.AddElement("3", "3", "5", "1", "1");
            model.AddElement("4", "4", "5", "1", "1");

            model.AddRestraint("1", true, true, true);
            model.AddRestraint("2", true, true, true);
            model.AddRestraint("3", true, true, true);
            model.AddRestraint("4", true, true, true);

            model.AddLoadCase("D");
            model.AddNodalForce("5", "D", 0, 0, -100);

            model.Build();

            model.RunAnalysis();

            model.PostAnalysis();

            Console.WriteLine($"Truss Forces -----------------------------------");
            foreach (var element in model.LineElements)
            {
                Console.WriteLine($"{element.Name,-5} {element.InternalForce,10:F2} {element.Stress,10:F2} {element.Elongation,15:E3}");
            }
            Console.WriteLine();

            //Console.WriteLine($"Reaction Forces -----------------------------------");
            //foreach (var node in model.Nodes)
            //{
            //    if (node.IsSupport.All(p => !p))
            //    {
            //        continue;
            //    }
            //    Console.WriteLine($"{node.Name,-5} {string.Join("\t", node.ReactionForce.Select(p => $"{p,10:F2}").ToList())}");
            //}
            //Console.WriteLine();

            //Console.WriteLine($"Resultant Displacements -----------------------------------");
            //foreach (var node in model.Nodes)
            //{
            //    if (node.IsSupport.All(p => p))
            //    {
            //        continue;
            //    }
            //    Console.WriteLine($"{node.Name,-5} {string.Join("\t", node.ResultantDisplacement.Select(p => $"{p,6:E3}").ToList())}");
            //}
            Console.WriteLine();
            Console.WriteLine("||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
        }

        static void Frame2D_1()
        {
            var model = new ModelFrame2D();
            model.AddNode("1", 0, 0);
            model.AddNode("2", 0, 1);

            model.AddMaterial("1", 100_000);
            model.AddProfileSection("1", 1_000, 100);

            model.AddElement("1", "1", "2", "1", "1");

            model.AddRestraint("1", true, true, false);
            model.AddRestraint("2", true, true, false);

            model.AddLoadCase("D");
            model.AddNodalForce("2", "D", 0, 0, 100);

            model.AddLoadCase("L");
            model.AddNodalForce("1", "L", 0, 0, -100);
            model.AddNodalForce("2", "L", 0, 0, 100);

            model.Build();

            model.RunAnalysis();

            model.PostAnalysis();

            Console.WriteLine($"Truss Forces -----------------------------------");
            foreach (var element in model.LineElements)
            {
                Console.WriteLine($"{element.Name,-5} {element.InternalForce,10:F2} {element.Stress,10:F2} {element.Elongation,15:E3}");
            }
            Console.WriteLine();

            Console.WriteLine($"Reaction Forces -----------------------------------");
            foreach (var node in model.Nodes)
            {
                if (node.IsSupport.All(p => !p))
                {
                    continue;
                }

                for (var j = 0; j < model.LoadCases.Count; j++)
                {
                    Console.WriteLine($"{node.Name,-5} {model.LoadCases[j],-5} {string.Join("\t", node.ReactionForce[j].Select(p => $"{p,10:F2}").ToList())}");
                }
            }
            Console.WriteLine();

            Console.WriteLine($"Resultant Displacements -----------------------------------");
            foreach (var node in model.Nodes)
            {
                if (node.IsSupport.All(p => p))
                {
                    continue;
                }

                for (var j = 0; j < model.LoadCases.Count; j++)
                {
                    Console.WriteLine($"{node.Name,-5} {model.LoadCases[j],-5} {string.Join("\t", node.ResultantDisplacement[j].Select(p => $"{p,6:E3}").ToList())}");
                }
            }
            Console.WriteLine();
            Console.WriteLine("||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
            Console.WriteLine();
            Console.WriteLine("||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
        }

        static void Frame3D_1()
        {
            var model = new ModelFrame3D();
            model.AddNode("1", 0, 0, 0);
            model.AddNode("2", 0, 1, 0);

            model.AddMaterial("1", 100_000);
            model.AddProfileSection("1", 1_000, 0.3, 100, 100, 100);

            model.AddElement("1", "1", "2", "1", "1");

            model.AddRestraint("1", true, true, true, true, true, true);
            model.AddRestraint("2", false, false, false, false, false, false);

            model.AddLoadCase("D");
            model.AddNodalForce("2", "D", 0, 100, 0, 0, 0, 0);

            model.Build();

            model.RunAnalysis();

            model.PostAnalysis();

            Console.WriteLine($"Truss Forces -----------------------------------");
            foreach (var element in model.LineElements)
            {
                Console.WriteLine($"{element.Name,-5} {element.InternalForce,10:F2} {element.Stress,10:F2} {element.Elongation,15:E3}");
            }
            Console.WriteLine();

            //Console.WriteLine($"Reaction Forces -----------------------------------");
            //foreach (var node in model.Nodes)
            //{
            //    if (node.IsSupport.All(p => !p))
            //    {
            //        continue;
            //    }
            //    Console.WriteLine($"{node.Name,-5} {string.Join("\t", node.ReactionForce.Select(p => $"{p,10:F2}").ToList())}");
            //}
            //Console.WriteLine();

            //Console.WriteLine($"Resultant Displacements -----------------------------------");
            //foreach (var node in model.Nodes)
            //{
            //    if (node.IsSupport.All(p => p))
            //    {
            //        continue;
            //    }
            //    Console.WriteLine($"{node.Name,-5} {string.Join("\t", node.ResultantDisplacement.Select(p => $"{p,6:E3}").ToList())}");
            //}
            Console.WriteLine();
            Console.WriteLine("||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
        }

        static void Plate_1()
        {
            var model = new FEA.Models.ModelPlate();
            model.AddNode("1", 0, 0);
            model.AddNode("2", 1, 0);
            model.AddNode("3", 1, 1);
            model.AddNode("4", 0, 1);

            model.AddMaterial("1", 1, 0.3);

            model.AddArea("1", "1", 10, "1", "2", "3", "4");

            model.AddRestraint("1", true, true, true);
            model.AddRestraint("2", true, true, true);
            model.AddRestraint("3", true, true, true);
            model.AddRestraint("4", true, true, true);

            model.AddLoadCase("D", AnalysisTypes.Linear);
            model.AddAreaLoad("1", "1", 100);

            model.Build();

            model.RunAnalysis();

            model.PostAnalysis();
        }
    }
}
