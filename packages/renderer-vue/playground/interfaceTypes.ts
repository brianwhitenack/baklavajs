import { NodeInterfaceType } from "@baklavajs/interface-types";
import "./interfaceTypes.css";
import { Part } from "./models/Part";
import { Measurement } from "./models/Measurement";

export const stringType = new NodeInterfaceType<string>("string");
export const numberType = new NodeInterfaceType<number>("number");
export const booleanType = new NodeInterfaceType<boolean>("boolean");
export const partType = new NodeInterfaceType<Part>("part");
export const measurementType = new NodeInterfaceType<Measurement>("measurement");
export const measurementListType = new NodeInterfaceType<Measurement[]>("measurementList");
export const partListType = new NodeInterfaceType<Part[]>("partList");

stringType.addConversion(numberType, (v) => parseFloat(v));
numberType.addConversion(stringType, (v) => (v !== null && v !== undefined && v.toString()) || "0");
booleanType.addConversion(stringType, (v) => (typeof v === "boolean" ? v.toString() : "null"));
partType.addConversion(partListType, (v) => (v ? [v] : []));
measurementType.addConversion(measurementListType, (v) => (v ? [v] : []));