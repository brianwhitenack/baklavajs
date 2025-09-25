import { defineNode, NodeInterface } from "@baklavajs/core";
import { setType } from "@baklavajs/interface-types";
import { stringType, booleanType } from "../interfaceTypes";
import { TextInputInterface } from "../../src";

export default defineNode({
    type: "FeatureFlagNode",
    title: "Feature Flag",
    inputs: {
        featureFlagName: () => new TextInputInterface("Feature Flag Name", "Feature Flag Name").use(setType, stringType),
    },
    outputs: {
        featureFlagValue: () => new NodeInterface<boolean>("Feature Flag Value", false).use(setType, booleanType),
    }
});