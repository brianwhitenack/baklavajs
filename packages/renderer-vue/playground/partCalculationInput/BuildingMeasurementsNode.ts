import { defineNode, NodeInterface } from "@baklavajs/core";
import { setType } from "@baklavajs/interface-types";
import { Measurement } from "../models/Measurement";
import { measurementListType } from "../interfaceTypes";

export default defineNode({
    type: "BuildingMeasurementsNode",
    title: "Building Measurements",
    inputs: {
    },
    outputs: {
        measurements: () => new NodeInterface<Measurement[]>("Measurements", []).use(setType, measurementListType),
    }
});