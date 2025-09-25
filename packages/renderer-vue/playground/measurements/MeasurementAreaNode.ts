import { defineNode, NodeInterface } from "@baklavajs/core";
import { setType } from "@baklavajs/interface-types";
import { Measurement } from "../models/Measurement";
import { numberType, measurementType } from "../interfaceTypes";

export default defineNode({
    type: "MeasurementAreaNode",
    title: "Area",
    inputs: {
        measurement: () => new NodeInterface<Measurement | undefined>("Measurement", undefined).use(setType, measurementType),
    },
    outputs: {
        area: () => new NodeInterface<number>("Area", 0).use(setType, numberType),
    },
    calculate(inputs) {
        return { area: inputs.measurement?.area };
    },
});