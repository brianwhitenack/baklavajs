import { defineNode, NodeInterface } from "@baklavajs/core";
import { Part } from "../models/Part";
import { setType } from "@baklavajs/interface-types";
import { partListType } from "../interfaceTypes";

export default defineNode({
    type: "PartCalculationOutputNode",
    title: "Calculation End",
    inputs: {
        parts: () => new NodeInterface<Part[]>("Parts", []).use(setType, partListType),
    }
});