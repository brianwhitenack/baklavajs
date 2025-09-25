import { defineNode, NodeInterface } from "@baklavajs/core";
import { NumberInterface, SelectInterface } from "../src";
import { setType } from "@baklavajs/interface-types";
import { numberType } from "./interfaceTypes";

export default defineNode({
    type: "MathNode",
    title: "Math",
    inputs: {
        operation: () => new SelectInterface("Operation", "Add", ["Add", "Subtract", "Divide"]).setPort(false),
        number1: () => new NumberInterface("Number", 1).use(setType, numberType),
        number2: () => new NumberInterface("Number", 10).use(setType, numberType),
    },
    outputs: {
        result: () => new NodeInterface<number>("Result", 0).use(setType, numberType),
    },
    calculate({ number1, number2, operation }) {
        let result: number;
        if (operation === "Add") {
            result = number1 + number2;
        }
        else if (operation === "Subtract")
        {
            result = number1 - number2;
        }
        else if (operation == "Divide")
        {
            result = number1 / number2;
        }
        else
        {
            throw new Error(`Unknown operation: ${operation}`);
        }
        return { result };
    },
});
