export class Part {
    sku: string;
    description: string;
    package: string;
    quantity: number;
    constructor(sku: string, description: string, packageType: string, quantity: number) {
        this.sku = sku;
        this.description = description;
        this.package = packageType;
        this.quantity = quantity;
    }
}