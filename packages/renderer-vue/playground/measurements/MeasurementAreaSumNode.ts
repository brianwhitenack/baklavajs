import { defineNode, NodeInterface } from "@baklavajs/core";
import { setType } from "@baklavajs/interface-types";
import { Measurement } from "../models/Measurement";
import { numberType, measurementListType } from "../interfaceTypes";

export default defineNode({
    type: "MeasurementAreaSumNode",
    title: "Area Sum",
    inputs: {
        measurements: () => new NodeInterface<Measurement[]>("Measurements", []).use(setType, measurementListType),
    },
    outputs: {
        areaSum: () => new NodeInterface<number>("Area Sum", 0).use(setType, numberType),
    },
    calculate(inputs) {
        // Sum up all the areas from the measurements array
        let areaSum = 0;

        // inputs.measurements should be an array of measurements
        if (Array.isArray(inputs.measurements)) {
            for (const measurement of inputs.measurements) {
                if (measurement && typeof measurement.area === 'number') {
                    areaSum += measurement.area;
                }
            }
        }

        return { areaSum };
    },
});