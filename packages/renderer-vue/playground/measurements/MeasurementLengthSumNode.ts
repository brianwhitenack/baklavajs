import { defineNode, NodeInterface } from "@baklavajs/core";
import { setType } from "@baklavajs/interface-types";
import { Measurement } from "../models/Measurement";
import { numberType, measurementListType } from "../interfaceTypes";

export default defineNode({
    type: "MeasurementLengthSumNode",
    title: "Length Sum",
    inputs: {
        measurements: () => new NodeInterface<Measurement[]>("Measurements", []).use(setType, measurementListType),
    },
    outputs: {
        lengthSum: () => new NodeInterface<number>("Length Sum", 0).use(setType, numberType),
    },
    calculate(inputs) {
        // Sum up all the lengths from the measurements array
        let lengthSum = 0;

        // inputs.measurements should be an array of measurements
        if (Array.isArray(inputs.measurements)) {
            for (const measurement of inputs.measurements) {
                if (measurement && typeof measurement.length === 'number') {
                    lengthSum += measurement.length;
                }
            }
        }

        return { lengthSum };
    },
});