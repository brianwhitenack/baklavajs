<template>
    <div id="app">
        <BaklavaEditor :view-model="baklavaView">
            <template #node="nodeProps">
                <CommentNodeRenderer v-if="nodeProps.node.type === 'CommentNode'" v-bind="nodeProps" />
                <NodeComponent v-else v-bind="nodeProps" />
            </template>
        </BaklavaEditor>
        <div class="controls-bar">
            <button @click="calculate">Calculate</button>
            <button @click="saveToFile">Save to File</button>
            <button @click="triggerFileInput">Load from File</button>
            <input ref="fileInput"
                   type="file"
                   accept=".json"
                   style="display: none"
                   @change="loadFromFile" />
            <button @click="setSelectItems">Set Select Items</button>
            <button @click="changeGridSize">Change Grid Size</button>
            <button @click="createSubgraph">Create Subgraph</button>
            <button @click="saveAndLoad">Save and Load</button>
            <button @click="changeSidebarWidth">SidebarWidth</button>
            <button @click="clearHistory">Clear History</button>
            <button @click="zoomToFitRandomNode">Zoom to Random Node</button>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { NodeInstanceOf } from "@baklavajs/core";
    import { BaklavaEditor, Components, SelectInterface, useBaklava, Commands, DEFAULT_TOOLBAR_COMMANDS } from "../src";
    import { DependencyEngine, applyResult } from "@baklavajs/engine";
    import { BaklavaInterfaceTypes } from "@baklavajs/interface-types";

    import TestNode from "./TestNode";
    import OutputNode from "./OutputNode";
    import BuilderTestNode from "./BuilderTestNode";
    import MathNode from "./MathNode";
    import AdvancedNode from "./AdvancedNode";
    import CommentNode from "./CommentNode";
    import InterfaceTestNode from "./InterfaceTestNode";
    import SelectTestNode from "./SelectTestNode";
    import SidebarNode from "./SidebarNode";
    import DynamicNode from "./DynamicNode";
    import UpdateTestNode from "./UpdateTestNode";
    import { DialogNode } from "./DialogNode";

    import ReactiveOutputTestNode from "./ReactiveOutputTestNode";

    import { stringType, numberType, booleanType, partType, partListType, measurementType, measurementListType } from "./interfaceTypes";

    import CommentNodeRenderer from "./CommentNodeRenderer.vue";
    import { defineComponent, h, ref } from "vue";
    import MultiInputNode from "./MultiInputNode";

    import MeasurementLengthNode from "./measurements/MeasurementLengthNode";
    import MeasurementAreaNode from "./measurements/MeasurementAreaNode";
    import MeasurementAreaSumNode from "./measurements/MeasurementAreaSumNode";
    import MeasurementLengthSumNode from "./measurements/MeasurementLengthSumNode";
    import FilterToTypeNode from "./measurements/FilterToTypeNode";
    import FilterToSelectionNode from "./measurements/FilterToSelectionNode";

    import BuildingMeasurementsNode from "./partCalculationInput/BuildingMeasurementsNode";
    import FeatureFlagNode from "./partCalculationInput/FeatureFlagNode";

    import CreatePartNode from "./parts/CreatePartNode";

    import StringVariableNode from "./variables/StringVariableNode";
    import NumberVariableNode from "./variables/NumberVariableNode";
    import partCalculationOutputNode from "./partCalculationOutput/partCalculationOutputNode";

    const NodeComponent = Components.Node;

    const token = Symbol("token");
    const baklavaView = useBaklava();
    const editor = baklavaView.editor;
    const fileInput = ref<HTMLInputElement>();

    (window as any).editor = baklavaView.editor;
    baklavaView.settings.enableMinimap = true;
    baklavaView.settings.sidebar.resizable = false;
    baklavaView.settings.palette.resizable = true;
    baklavaView.settings.displayValueOnHover = true;
    baklavaView.settings.nodes.resizable = true;
    baklavaView.settings.nodes.reverseY = false;
    baklavaView.settings.contextMenu.additionalItems = [
        { isDivider: true },
        { label: "Copy", command: Commands.COPY_COMMAND },
        { label: "Paste", command: Commands.PASTE_COMMAND },
    ];

    const CLEAR_ALL_COMMAND = "CLEAR_ALL";
    baklavaView.commandHandler.registerCommand(CLEAR_ALL_COMMAND, {
        execute: () => {
            baklavaView.displayedGraph.nodes.forEach((node) => {
                baklavaView.displayedGraph.removeNode(node);
            });
        },
        canExecute: () => baklavaView.displayedGraph.nodes.length > 0,
    });
    baklavaView.settings.toolbar.commands = [
        ...DEFAULT_TOOLBAR_COMMANDS,
        {
            command: CLEAR_ALL_COMMAND,
            title: "Clear All",
            icon: defineComponent(() => {
                return () => h("div", "Clear All");
            }),
        },
    ];

    const engine = new DependencyEngine(editor);
    engine.events.afterRun.subscribe(token, (r) => {
        engine.pause();
        applyResult(r, editor);
        engine.resume();
        console.log(r);
    });
    engine.hooks.gatherCalculationData.subscribe(token, () => "def");
    engine.start();

    const nodeInterfaceTypes = new BaklavaInterfaceTypes(editor, { viewPlugin: baklavaView, engine });
    nodeInterfaceTypes.addTypes(stringType, numberType, booleanType, partType, partListType, measurementType, measurementListType);

    editor.registerNodeType(CreatePartNode, { category: "1 Parts" });
    editor.registerNodeType(StringVariableNode, { category: "2 Variables" });
    editor.registerNodeType(NumberVariableNode, { category: "2 Variables" });
    editor.registerNodeType(MathNode, { category: "3 Math" });
    editor.registerNodeType(MeasurementLengthNode, { category: "4 Measurements" });
    editor.registerNodeType(MeasurementLengthSumNode, { category: "4 Measurements" });
    editor.registerNodeType(MeasurementAreaNode, { category: "4 Measurements" });
    editor.registerNodeType(MeasurementAreaSumNode, { category: "4 Measurements" });
    editor.registerNodeType(FilterToTypeNode, { category: "4 Measurements" });
    editor.registerNodeType(FilterToSelectionNode, { category: "4 Measurements" });

    editor.registerNodeType(BuildingMeasurementsNode, { category: "5 Part Calculation Input" });
    editor.registerNodeType(FeatureFlagNode, { category: "5 Part Calculation Input" });
    editor.registerNodeType(partCalculationOutputNode, { category: "6 Part Calculation Output" });

    editor.registerNodeType(TestNode, { category: "Examples" });
    editor.registerNodeType(OutputNode, { category: "Examples" });
    editor.registerNodeType(BuilderTestNode, { category: "Examples" });
    editor.registerNodeType(DialogNode, { category: "Examples" });
    editor.registerNodeType(AdvancedNode, { category: "Examples" });
    editor.registerNodeType(CommentNode, { category: "Examples" });
    editor.registerNodeType(InterfaceTestNode, { category: "Examples" });
    editor.registerNodeType(SelectTestNode, { category: "Examples" });
    editor.registerNodeType(SidebarNode, { category: "Examples" });
    editor.registerNodeType(DynamicNode, { category: "Examples" });
    editor.registerNodeType(UpdateTestNode, { category: "Examples" });
    editor.registerNodeType(ReactiveOutputTestNode, { category: "Examples" });
    editor.registerNodeType(MultiInputNode, { category: "Examples" });

    const calculate = async () => {
        console.log(await engine.runOnce("def"));
    };

    const saveToFile = async () => {
        const state = JSON.stringify(editor.save(), null, 2);
        const blob = new Blob([state], { type: "application/json" });

        // Check if the File System Access API is available (Chrome, Edge, etc.)
        if ('showSaveFilePicker' in window) {
            try {
                const handle = await (window as any).showSaveFilePicker({
                    suggestedName: `baklava-graph-${new Date().toISOString().slice(0, 10)}.json`,
                    types: [{
                        description: 'JSON Files',
                        accept: { 'application/json': ['.json'] }
                    }]
                });
                const writable = await handle.createWritable();
                await writable.write(blob);
                await writable.close();
                console.log("Graph saved to file");
                return;
            } catch (err) {
                // User cancelled the save dialog
                if (err.name === 'AbortError') {
                    console.log("Save cancelled");
                    return;
                }
                // Fall through to the legacy method if there's an error
                console.warn("File System Access API failed, using fallback", err);
            }
        }

        // Fallback for browsers that don't support File System Access API
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = `baklava-graph-${new Date().toISOString().slice(0, 10)}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        console.log("Graph saved to file (fallback method)");
    };

    const triggerFileInput = () => {
        fileInput.value?.click();
    };

    const loadFromFile = (event: Event) => {
        const target = event.target as HTMLInputElement;
        const file = target.files?.[0];

        if (!file) {
            return;
        }

        const reader = new FileReader();
        reader.onload = (e) => {
            try {
                const content = e.target?.result as string;
                const state = JSON.parse(content);
                editor.load(state);
                console.log("Graph loaded from file:", file.name);
            } catch (error) {
                console.error("Failed to load graph from file:", error);
                alert("Failed to load file. Please make sure it's a valid Baklava graph JSON file.");
            }
        };
        reader.readAsText(file);

        // Reset the input so the same file can be loaded again
        target.value = "";
    };

    // Auto-load from localStorage on startup (optional - uncomment if needed)
    // const loadFromLocalStorage = () => {
    //     const state = window.localStorage.getItem("state");
    //     if (state) {
    //         try {
    //             editor.load(JSON.parse(state));
    //             console.log("Loaded state from localStorage");
    //         } catch (e) {
    //             console.error(e);
    //         }
    //     }
    // };
    // loadFromLocalStorage();

    const saveAndLoad = () => {
        editor.load(editor.save());
    };

    const setSelectItems = () => {
        for (const node of editor.graph.nodes) {
            if (node.type === "SelectTestNode") {
                const n = node as unknown as NodeInstanceOf<typeof SelectTestNode>;
                const sel = n.inputs.advanced as SelectInterface<number | undefined>;
                sel.items = [
                    { text: "X", value: 1 },
                    { text: node.id, value: 2 },
                ];
            }
        }
    };

    const changeGridSize = () => {
        baklavaView.settings.background.gridSize = Math.round(Math.random() * 100) + 100;
    };

    const createSubgraph = () => {
        baklavaView.commandHandler.executeCommand<Commands.CreateSubgraphCommand>(Commands.CREATE_SUBGRAPH_COMMAND);
    };

    const changeSidebarWidth = () => {
        baklavaView.settings.sidebar.width = Math.round(Math.random() * 500) + 300;
        baklavaView.settings.sidebar.resizable = !baklavaView.settings.sidebar.resizable;
    };

    const clearHistory = () => {
        baklavaView.commandHandler.executeCommand<Commands.ClearHistoryCommand>(Commands.CLEAR_HISTORY_COMMAND);
    };

    const zoomToFitRandomNode = () => {
        if (baklavaView.displayedGraph.nodes.length === 0) {
            return;
        }

        const nodes = baklavaView.displayedGraph.nodes;
        const node = nodes[Math.floor(Math.random() * nodes.length)];
        baklavaView.commandHandler.executeCommand<Commands.ZoomToFitNodesCommand>(
            Commands.ZOOM_TO_FIT_NODES_COMMAND,
            true,
            [node],
        );
    };
</script>

<style>
    #app {
        margin: 0;
        height: 100vh;
        width: 100vw;
        display: flex;
        flex-direction: column;
    }

    .baklava-editor {
        flex: 1;
        overflow: hidden;
    }

    .controls-bar {
        background-color: #1b202c;
        border-top: 1px solid #2c3748;
        padding: 10px;
        display: flex;
        gap: 8px;
        align-items: center;
        flex-wrap: wrap;
    }

    .controls-bar button {
        background-color: #2c3748;
        color: white;
        border: none;
        padding: 8px 16px;
        border-radius: 3px;
        cursor: pointer;
        font-size: 14px;
        transition: background-color 0.2s;
    }

    .controls-bar button:hover {
        background-color: #455670;
    }

    .controls-bar button:active {
        background-color: #556986;
    }
</style>
