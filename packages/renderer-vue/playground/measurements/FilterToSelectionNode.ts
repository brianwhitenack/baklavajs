import { defineNode, NodeInterface } from "@baklavajs/core";
import { setType } from "@baklavajs/interface-types";
import { Measurement } from "../models/Measurement";
import { measurementListType, stringType } from "../interfaceTypes";
import { TextInputInterface } from "../../src";

export default defineNode({
    type: "FilterToSelectionNode",
    title: "Filter To Selection",
    inputs: {
        inputMeasurements: () => new NodeInterface<Measurement[]>("Measurements", []).use(setType, measurementListType),
        selectionName: () => new TextInputInterface("Selection Name", "").use(setType, stringType),
        selectionValue: () => new NodeInterface("Selection Value", ""),
    },
    outputs: {
        outputMeasurements: () => new NodeInterface<Measurement[]>("Filtered Measurements", []).use(setType, measurementListType),
    },
    calculate(inputs) {
        const outputMeasurements: Measurement[] = [];
        if (Array.isArray(inputs.inputMeasurements)) {
            for (const measurement of inputs.inputMeasurements) {
                // FIX
                if (measurement && typeof measurement.type === 'string') {
                    outputMeasurements.push(measurement);
                }
            }
        }

        return { outputMeasurements };
    },
});