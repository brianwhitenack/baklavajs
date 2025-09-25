import { defineNode } from "@baklavajs/core";
import { setType } from "@baklavajs/interface-types";
import { numberType } from "../interfaceTypes";
import { NumberInterface } from "../../src";

export default defineNode({
    type: "NumberVariable",
    title: "Number",
    outputs: {
        result: () => new NumberInterface("Value", 0).use(setType, numberType),
    }
});