import { defineNode, NodeInterface } from "@baklavajs/core";
import { setType } from "@baklavajs/interface-types";
import { TextInputInterface, NumberInterface } from "../../src";
import { numberType, stringType, partType } from "../interfaceTypes";
import { Part } from "../models/Part";

export default defineNode({
    type: "CreatePartNode",
    title: "Create Part",
    inputs: {
        sku: () => new TextInputInterface("SKU", "").use(setType, stringType),
        description: () => new TextInputInterface("Description", "").use(setType, stringType),
        package: () => new TextInputInterface("Package", "").use(setType, stringType),
        quantity: () => new NumberInterface("Quantity", 0).use(setType, numberType),
    },
    outputs: {
        part: () => new NodeInterface<Part | undefined>("Part", undefined).use(setType, partType),
    },
    calculate(inputs) {
        return { part: new Part(inputs.sku, inputs.description, inputs.package, inputs.quantity) };
    },
});