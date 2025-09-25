import { defineNode, NodeInterface } from "@baklavajs/core";
import { setType } from "@baklavajs/interface-types";
import { Measurement } from "../models/Measurement";
import { measurementListType, stringType } from "../interfaceTypes";
import { TextInputInterface } from "../../src";

export default defineNode({
    type: "FilterToTypeNode",
    title: "Filter To Type",
    inputs: {
        inputMeasurements: () => new NodeInterface<Measurement[]>("Measurements", []).use(setType, measurementListType),
        type: () => new TextInputInterface("measurement type", "").use(setType, stringType),
    },
    outputs: {
        outputMeasurements: () => new NodeInterface<Measurement[]>("Filtered Measurements", []).use(setType, measurementListType),
    },
    calculate(inputs) {
        const outputMeasurements: Measurement[] = [];
        if (Array.isArray(inputs.inputMeasurements)) {
            for (const measurement of inputs.inputMeasurements) {
                if (measurement && typeof measurement.type === 'string' && measurement.type == inputs.type) {
                    outputMeasurements.push(measurement);
                }
            }
        }

        return { outputMeasurements };
    },
});