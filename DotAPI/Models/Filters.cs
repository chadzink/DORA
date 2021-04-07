namespace DORA.DotAPI.Models
{
    public enum FilterFieldType
    {
        String,
        Number,
        Decimal,
        Float,
        Date,
        None
    }

    public class FilterField
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Operator { get; set; }
    }

    public class FilterOperatorOptions
    {
        public string Description { get; set; }
        public string Value { get; set; }
    }
}
