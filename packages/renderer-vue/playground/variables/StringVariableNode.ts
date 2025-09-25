import { defineNode } from "@baklavajs/core";
import { TextInputInterface } from "../../src";
import { setType } from "@baklavajs/interface-types";
import { stringType } from "../interfaceTypes";

export default defineNode({
    type: "StringVariable",
    title: "String",
    outputs: {
        result: () => new TextInputInterface("Value", "Value").use(setType, stringType),
    }
});