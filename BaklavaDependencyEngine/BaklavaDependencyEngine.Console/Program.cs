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
                System.Console.WriteLine("Choose sample graph:");
                System.Console.WriteLine("1. Basic Math Sample (default)");
                System.Console.WriteLine("2. Part Creation Sample");
                System.Console.WriteLine("3. Building Measurements Sample");
                System.Console.WriteLine("4. Filter Nodes Sample");
                var choice = System.Console.ReadLine();

                graph = choice switch
                {
                    "2" => CreatePartSampleGraph(),
                    "3" => CreateBuildingMeasurementGraph(),
                    "4" => CreateFilterSampleGraph(),
                    _ => CreateSampleGraph()
                };
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

            // Only wait for key if in interactive mode
            if (System.Console.IsInputRedirected == false)
            {
                System.Console.WriteLine("\nPress any key to exit...");
                System.Console.ReadKey();
            }
        }

        private static async Task<Graph> LoadGraphFromFile(string path)
        {
            string graphJson = await File.ReadAllTextAsync(path);

            // Use V3 serializer with JsonSubTypes
            var graphSerializer = new GraphSerializerV3();
            return graphSerializer.DeserializeGraph(graphJson);
        }

        // No longer needed with V2 serializer - types are discovered automatically
        // private static void RegisterNodeTypes(GraphSerializer serializer)

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

        static Graph CreatePartSampleGraph()
        {
            var graph = new Graph();

            // Create Part Creation nodes
            var skuVar = new StringVariableNode();
            skuVar.Outputs["result"].Value = "ABC-123";
            skuVar.Title = "SKU Variable";

            var descVar = new StringVariableNode();
            descVar.Outputs["result"].Value = "Sample Component";
            descVar.Title = "Description Variable";

            var pkgVar = new StringVariableNode();
            pkgVar.Outputs["result"].Value = "Standard Package";
            pkgVar.Title = "Package Variable";

            var qtyVar = new NumberVariableNode();
            qtyVar.Outputs["result"].Value = 10.0;
            qtyVar.Title = "Quantity Variable";

            var createPartNode = new CreatePartNode();
            var outputNode = new PartCalculationOutputNode();

            // Add all nodes
            graph.AddNode(skuVar);
            graph.AddNode(descVar);
            graph.AddNode(pkgVar);
            graph.AddNode(qtyVar);
            graph.AddNode(createPartNode);
            graph.AddNode(outputNode);

            // Create connections
            graph.AddConnection(new Connection(skuVar.Outputs["result"], createPartNode.Inputs["sku"]));
            graph.AddConnection(new Connection(descVar.Outputs["result"], createPartNode.Inputs["description"]));
            graph.AddConnection(new Connection(pkgVar.Outputs["result"], createPartNode.Inputs["package"]));
            graph.AddConnection(new Connection(qtyVar.Outputs["result"], createPartNode.Inputs["quantity"]));

            // Note: This would normally connect to partListType, but for demo we'll assume the conversion works
            graph.AddConnection(new Connection(createPartNode.Outputs["part"], outputNode.Inputs["parts"]));

            return graph;
        }

        static Graph CreateBuildingMeasurementGraph()
        {
            var graph = new Graph();

            // Building measurements input
            var buildingMeasurements = new BuildingMeasurementsNode();

            // Sum nodes
            var areaSumNode = new MeasurementAreaSumNode();
            var lengthSumNode = new MeasurementLengthSumNode();

            // Display nodes
            var areaDisplay = new DisplayNode();
            areaDisplay.Title = "Total Area Display";

            var lengthDisplay = new DisplayNode();
            lengthDisplay.Title = "Total Length Display";

            // Add nodes to graph
            graph.AddNode(buildingMeasurements);
            graph.AddNode(areaSumNode);
            graph.AddNode(lengthSumNode);
            graph.AddNode(areaDisplay);
            graph.AddNode(lengthDisplay);

            // Connect measurements to sum nodes
            graph.AddConnection(new Connection(
                buildingMeasurements.Outputs["measurements"],
                areaSumNode.Inputs["measurements"]));

            graph.AddConnection(new Connection(
                buildingMeasurements.Outputs["measurements"],
                lengthSumNode.Inputs["measurements"]));

            // Connect sums to displays
            graph.AddConnection(new Connection(
                areaSumNode.Outputs["areaSum"],
                areaDisplay.Inputs["value"]));

            graph.AddConnection(new Connection(
                lengthSumNode.Outputs["lengthSum"],
                lengthDisplay.Inputs["value"]));

            return graph;
        }

        static Graph CreateFilterSampleGraph()
        {
            var graph = new Graph();

            // Building measurements input
            var buildingMeasurements = new BuildingMeasurementsNode();
            buildingMeasurements.Title = "All Measurements";

            // Filter by type node
            var filterToType = new FilterToTypeNode();
            filterToType.Title = "Filter to Wall Type";
            filterToType.Inputs["type"].Value = "Wall";

            // Filter by selection node
            var filterToSelection = new FilterToSelectionNode();
            filterToSelection.Title = "Filter to Room 101";
            filterToSelection.Inputs["selectionName"].Value = "Room";
            filterToSelection.Inputs["selectionValue"].Value = "Room 101";

            // Sum nodes for filtered results
            var filteredAreaSum = new MeasurementAreaSumNode();
            filteredAreaSum.Title = "Wall Area Sum";

            var roomAreaSum = new MeasurementAreaSumNode();
            roomAreaSum.Title = "Room 101 Area Sum";

            // Display nodes
            var wallAreaDisplay = new DisplayNode();
            wallAreaDisplay.Title = "Total Wall Area";

            var roomAreaDisplay = new DisplayNode();
            roomAreaDisplay.Title = "Total Room 101 Area";

            // Add nodes to graph
            graph.AddNode(buildingMeasurements);
            graph.AddNode(filterToType);
            graph.AddNode(filterToSelection);
            graph.AddNode(filteredAreaSum);
            graph.AddNode(roomAreaSum);
            graph.AddNode(wallAreaDisplay);
            graph.AddNode(roomAreaDisplay);

            // Connect measurements to filters
            graph.AddConnection(new Connection(
                buildingMeasurements.Outputs["measurements"],
                filterToType.Inputs["inputMeasurements"]));

            graph.AddConnection(new Connection(
                buildingMeasurements.Outputs["measurements"],
                filterToSelection.Inputs["inputMeasurements"]));

            // Connect filtered results to sum nodes
            graph.AddConnection(new Connection(
                filterToType.Outputs["outputMeasurements"],
                filteredAreaSum.Inputs["measurements"]));

            graph.AddConnection(new Connection(
                filterToSelection.Outputs["outputMeasurements"],
                roomAreaSum.Inputs["measurements"]));

            // Connect sums to displays
            graph.AddConnection(new Connection(
                filteredAreaSum.Outputs["areaSum"],
                wallAreaDisplay.Inputs["value"]));

            graph.AddConnection(new Connection(
                roomAreaSum.Outputs["areaSum"],
                roomAreaDisplay.Inputs["value"]));

            return graph;
        }

        static void DemonstrateSerializer(Graph graph)
        {
            // Use V3 serializer with JsonSubTypes
            var serializer = new GraphSerializerV3();

            // Serialize the graph (with TypeScript-compatible format)
            var json = serializer.SerializeGraph(graph, wrapInGraphFile: true);
            System.Console.WriteLine("Graph serialized to JSON (TypeScript-compatible format):");
            System.Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + "...\n");

            // Save to file
            var fileName = "sample_graph.json";
            File.WriteAllText(fileName, json);
            System.Console.WriteLine($"Graph saved to {fileName}");

            // Load and deserialize
            var loadedJson = File.ReadAllText(fileName);
            try
            {
                var loadedGraph = serializer.DeserializeGraph(loadedJson);
                System.Console.WriteLine($"Graph loaded successfully. Contains {loadedGraph.Nodes.Count} nodes and {loadedGraph.Connections.Count} connections.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Failed to deserialize: {ex.Message}");
            }
        }
    }
}