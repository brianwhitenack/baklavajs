using System;
using System.IO;
using System.Threading.Tasks;
using BaklavaDependencyEngine.Core;
using BaklavaDependencyEngine.Core.Models;
using BaklavaDependencyEngine.Core.ExampleNodes;
using BaklavaDependencyEngine.Core.Serialization;

namespace BaklavaDependencyEngine.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Console.WriteLine("Enter custom graph path or leave blank for sample:");
            string? path = System.Console.ReadLine();

            Graph graph;
            if (path != null && File.Exists(path))
            {
                graph = await LoadGraphFromFile(path);
            }
            else
            {
                graph = CreateSampleGraph();
            }

            System.Console.WriteLine("Baklava Dependency Engine - C# Implementation");
            System.Console.WriteLine("=============================================\n");

            // Create a simple calculation graph

            // Create and configure the engine
            var engine = new DependencyEngine();

            // Subscribe to events
            engine.BeforeNodeCalculation += (sender, e) =>
            {
                System.Console.WriteLine($"Calculating node: {e.Node.Title} ({e.Node.Type})");
            };

            engine.AfterNodeCalculation += (sender, e) =>
            {
                System.Console.WriteLine($"  Completed: {e.Node.Title}");
                foreach (var output in e.Values)
                {
                    System.Console.WriteLine($"    {output.Key} = {output.Value}");
                }
            };

            // Run the calculation
            System.Console.WriteLine("Running graph calculation...\n");
            try
            {
                var result = await engine.RunGraph(graph);

                System.Console.WriteLine("\n=== Final Results ===");
                foreach (var nodeResult in result)
                {
                    var node = graph.Nodes.Find(n => n.Id == nodeResult.Key);
                    System.Console.WriteLine($"\nNode: {node?.Title ?? nodeResult.Key}");
                    foreach (var output in nodeResult.Value)
                    {
                        System.Console.WriteLine($"  {output.Key}: {output.Value}");
                    }
                }
            }
            catch (CycleException)
            {
                System.Console.WriteLine("ERROR: Cycle detected in the graph!");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"ERROR: {ex.Message}");
            }

            // Demonstrate serialization
            System.Console.WriteLine("\n=== Serialization Demo ===");
            DemonstrateSerializer(graph);

            System.Console.WriteLine("\nPress any key to exit...");
            System.Console.ReadKey();
        }

        private static async Task<Graph> LoadGraphFromFile(string path)
        {
            string graphJson = await File.ReadAllTextAsync(path);
            GraphSerializer graphSerializer = new GraphSerializer();
            return graphSerializer.DeserializeGraph(graphJson);
        }

        static Graph CreateSampleGraph()
        {
            var graph = new Graph();

            // Create nodes
            var number1 = new NumberNode();
            number1.Outputs["value"].Value = 10.0;
            number1.Title = "Number 1";

            var number2 = new NumberNode();
            number2.Outputs["value"].Value = 5.0;
            number2.Title = "Number 2";

            var addNode = new MathNode();
            addNode.MathOperation = MathNode.Operation.Add;
            addNode.Title = "Add";

            var multiplyNode = new MathNode();
            multiplyNode.MathOperation = MathNode.Operation.Multiply;
            multiplyNode.Title = "Multiply by 2";

            var number3 = new NumberNode();
            number3.Outputs["value"].Value = 2.0;
            number3.Title = "Multiplier";

            var displayNode = new DisplayNode();
            displayNode.Title = "Final Output";

            // Add nodes to graph
            graph.AddNode(number1);
            graph.AddNode(number2);
            graph.AddNode(addNode);
            graph.AddNode(multiplyNode);
            graph.AddNode(number3);
            graph.AddNode(displayNode);

            // Create connections
            // number1 -> addNode.number1
            graph.AddConnection(new Connection(
                number1.Outputs["value"],
                addNode.Inputs["number1"]));

            // number2 -> addNode.number2
            graph.AddConnection(new Connection(
                number2.Outputs["value"],
                addNode.Inputs["number2"]));

            // addNode.result -> multiplyNode.number1
            graph.AddConnection(new Connection(
                addNode.Outputs["result"],
                multiplyNode.Inputs["number1"]));

            // number3 -> multiplyNode.number2
            graph.AddConnection(new Connection(
                number3.Outputs["value"],
                multiplyNode.Inputs["number2"]));

            // multiplyNode.result -> displayNode.value
            graph.AddConnection(new Connection(
                multiplyNode.Outputs["result"],
                displayNode.Inputs["value"]));

            return graph;
        }

        static void DemonstrateSerializer(Graph graph)
        {
            var serializer = new GraphSerializer();

            // Register node types for deserialization
            serializer.RegisterNodeType<NumberNode>("NumberNode");
            serializer.RegisterNodeType<MathNode>("MathNode");
            serializer.RegisterNodeType<DisplayNode>("DisplayNode");

            // Serialize the graph
            var json = serializer.SerializeGraph(graph);
            System.Console.WriteLine("Graph serialized to JSON:");
            System.Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + "...\n");

            // Save to file
            var fileName = "sample_graph.json";
            File.WriteAllText(fileName, json);
            System.Console.WriteLine($"Graph saved to {fileName}");

            // Load and deserialize
            var loadedJson = File.ReadAllText(fileName);
            var loadedGraph = serializer.DeserializeGraph(loadedJson);
            System.Console.WriteLine($"Graph loaded successfully. Contains {loadedGraph.Nodes.Count} nodes and {loadedGraph.Connections.Count} connections.");
        }
    }
}