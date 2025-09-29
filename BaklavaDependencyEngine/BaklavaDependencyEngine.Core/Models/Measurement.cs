namespace BaklavaDependencyEngine.Core.Models
{
    public class Measurement
    {
        public string Type { get; set; }
        public double Length { get; set; }
        public double Area { get; set; }
        public Dictionary<string, object> Selections { get; set; }

        public Measurement()
        {
            Type = string.Empty;
            Length = 0.0;
            Area = 0.0;
            Selections = new Dictionary<string, object>();
        }

        public Measurement(string type, double length, double area)
        {
            Type = type;
            Length = length;
            Area = area;
            Selections = new Dictionary<string, object>();
        }

        public override string ToString()
        {
            return $"Measurement: {Type} (Length: {Length:F2}, Area: {Area:F2})";
        }

        public override bool Equals(object obj)
        {
            if (obj is Measurement other)
            {
                return Type == other.Type &&
                       Math.Abs(Length - other.Length) < 0.0001 &&
                       Math.Abs(Area - other.Area) < 0.0001;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Length, Area);
        }
    }
}