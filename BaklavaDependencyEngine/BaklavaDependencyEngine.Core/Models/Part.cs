using System;

namespace BaklavaDependencyEngine.Core.Models
{
    public class Part
    {
        public string Sku { get; set; }
        public string Description { get; set; }
        public string Package { get; set; }
        public int Quantity { get; set; }

        public Part(string sku, string description, string packageType, int quantity)
        {
            Sku = sku;
            Description = description;
            Package = packageType;
            Quantity = quantity;
        }

        public Part()
        {
            Sku = string.Empty;
            Description = string.Empty;
            Package = string.Empty;
            Quantity = 0;
        }

        public override string ToString()
        {
            return $"Part: {Sku} - {Description} ({Package}) x{Quantity}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Part other)
            {
                return Sku == other.Sku &&
                       Description == other.Description &&
                       Package == other.Package &&
                       Quantity == other.Quantity;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Sku, Description, Package, Quantity);
        }
    }
}