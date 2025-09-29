using BaklavaDependencyEngine.Core;
using BaklavaDependencyEngine.Core.ExampleNodes;
using BaklavaDependencyEngine.Core.Models;
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
                string choice = System.Console.ReadLine();

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
            DependencyEngine engine = new DependencyEngine();

            // Subscribe to events
            engine.BeforeNodeCalculation += (sender, e) =>
            {
                System.Console.WriteLine($"Calculating node: {e.Node.Title} ({e.Node.Type})");
            };

            engine.AfterNodeCalculation += (sender, e) =>
            {
                System.Console.WriteLine($"  Completed: {e.Node.Title}");
                foreach (KeyValuePair<string, object> output in e.Values)
                {
                    System.Console.WriteLine($"    {output.Key} = {output.Value}");
                }
            };

            // Run the calculation
            System.Console.WriteLine("Running graph calculation...\n");
            try
            {
                CalculationResult result = await engine.RunGraph(graph);

                System.Console.WriteLine("\n=== Final Results ===");
                foreach (KeyValuePair<string, Dictionary<string, object>> nodeResult in result)
                {
                    Node node = graph.Nodes.Find(n => n.Id == nodeResult.Key);
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
            GraphSerializer graphSerializer = new GraphSerializer();
            return graphSerializer.DeserializeGraph(graphJson);
        }

        // No longer needed with V2 serializer - types are discovered automatically
        // private static void RegisterNodeTypes(GraphSerializer serializer)

        static Graph CreateSampleGraph()
        {
            Graph graph = new Graph();

            // Create nodes
            NumberNode number1 = new NumberNode();
            number1.Outputs["value"].Value = 10.0;
            number1.Title = "Number 1";

            NumberNode number2 = new NumberNode();
            number2.Outputs["value"].Value = 5.0;
            number2.Title = "Number 2";

            MathNode addNode = new MathNode();
            addNode.MathOperation = MathNode.Operation.Add;
            addNode.Title = "Add";

            MathNode multiplyNode = new MathNode();
            multiplyNode.MathOperation = MathNode.Operation.Multiply;
            multiplyNode.Title = "Multiply by 2";

            NumberNode number3 = new NumberNode();
            number3.Outputs["value"].Value = 2.0;
            number3.Title = "Multiplier";

            DisplayNode displayNode = new DisplayNode();
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
            Graph graph = new Graph();

            // Create Part Creation nodes
            StringVariableNode skuVar = new StringVariableNode();
            skuVar.Outputs["result"].Value = "ABC-123";
            skuVar.Title = "SKU Variable";

            StringVariableNode descVar = new StringVariableNode();
            descVar.Outputs["result"].Value = "Sample Component";
            descVar.Title = "Description Variable";

            StringVariableNode pkgVar = new StringVariableNode();
            pkgVar.Outputs["result"].Value = "Standard Package";
            pkgVar.Title = "Package Variable";

            NumberVariableNode qtyVar = new NumberVariableNode();
            qtyVar.Outputs["result"].Value = 10.0;
            qtyVar.Title = "Quantity Variable";

            CreatePartNode createPartNode = new CreatePartNode();
            PartCalculationOutputNode outputNode = new PartCalculationOutputNode();

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
            Graph graph = new Graph();

            // Building measurements input
            BuildingMeasurementsNode buildingMeasurements = new BuildingMeasurementsNode();

            // Sum nodes
            MeasurementAreaSumNode areaSumNode = new MeasurementAreaSumNode();
            MeasurementLengthSumNode lengthSumNode = new MeasurementLengthSumNode();

            // Display nodes
            DisplayNode areaDisplay = new DisplayNode();
            areaDisplay.Title = "Total Area Display";

            DisplayNode lengthDisplay = new DisplayNode();
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
            BuildingMeasurementsNode buildingMeasurements = new BuildingMeasurementsNode();
            buildingMeasurements.Title = "All Measurements";

            // Filter by type node
            FilterToTypeNode filterToType = new FilterToTypeNode();
            filterToType.Title = "Filter to Wall Type";
            filterToType.Inputs["type"].Value = "Wall";

            // Filter by selection node
            FilterToSelectionNode filterToSelection = new FilterToSelectionNode();
            filterToSelection.Title = "Filter to Room 101";
            filterToSelection.Inputs["selectionName"].Value = "Room";
            filterToSelection.Inputs["selectionValue"].Value = "Room 101";

            // Sum nodes for filtered results
            MeasurementAreaSumNode filteredAreaSum = new MeasurementAreaSumNode();
            filteredAreaSum.Title = "Wall Area Sum";

            MeasurementAreaSumNode roomAreaSum = new MeasurementAreaSumNode();
            roomAreaSum.Title = "Room 101 Area Sum";

            // Display nodes
            DisplayNode wallAreaDisplay = new DisplayNode();
            wallAreaDisplay.Title = "Total Wall Area";

            DisplayNode roomAreaDisplay = new DisplayNode();
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
            GraphSerializer serializer = new GraphSerializer();

            // Serialize the graph (with TypeScript-compatible format)
            string json = serializer.SerializeGraph(graph);
            System.Console.WriteLine("Graph serialized to JSON (TypeScript-compatible format):");
            System.Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)) + "...\n");

            // Save to file
            string fileName = "sample_graph.json";
            File.WriteAllText(fileName, json);
            System.Console.WriteLine($"Graph saved to {fileName}");

            // Load and deserialize
            string loadedJson = File.ReadAllText(fileName);
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