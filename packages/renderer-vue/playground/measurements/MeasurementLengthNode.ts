import { defineNode, NodeInterface } from "@baklavajs/core";
import { setType } from "@baklavajs/interface-types";
import { Measurement } from "../models/Measurement";
import { numberType, measurementType } from "../interfaceTypes";

export default defineNode({
    type: "MeasurementLengthNode",
    title: "Length",
    inputs: {
        measurement: () => new NodeInterface<Measurement | undefined>("Measurement", undefined).use(setType, measurementType),
    },
    outputs: {
        length: () => new NodeInterface<number>("Length", 0).use(setType, numberType),
    },
    calculate(inputs) {
        return { length: inputs.measurement?.length };
    },
});