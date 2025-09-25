export class Measurement {
	type: string;
	length: number;
	area: number;
	selections: Record<string, any>;
	constructor() {
		this.type = "";
		this.length = 0;
		this.area = 0;
		this.selections = {};
	}
}